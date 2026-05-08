using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.Json;



namespace SlotCopyApp
{
    public class GlobalKeyboardHook
    {
        private IntPtr _hookID = IntPtr.Zero;
        private Win32Interop.LowLevelKeyboardProc _proc;

        private Dictionary<int, string> _slots = new Dictionary<int, string>();
        private Stopwatch _copyWindowTimer = new Stopwatch();
        private Stopwatch _vHoldTimer = new Stopwatch();

        private bool _isCtrlDown = false;
        private bool _isVDown = false;
        private bool _slotTriggered = false;
        private bool _suppressNextV = false;

        private static readonly SemaphoreSlim _clipboardLock = new SemaphoreSlim(1, 1);
        private string _savePath;

        public GlobalKeyboardHook()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "SlotCopyApp");
            Directory.CreateDirectory(appFolder);
            _savePath = Path.Combine(appFolder, "slots.json");

            LoadFromDisk();
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                int eventType = (int)wParam;

                bool isRelevant = (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.ControlKey ||
                                   key == Keys.C || key == Keys.V || (key >= Keys.D1 && key <= Keys.D9));

                if (!isRelevant) return Win32Interop.CallNextHookEx(_hookID, nCode, wParam, lParam);
                if (_suppressNextV && key == Keys.V) return Win32Interop.CallNextHookEx(_hookID, nCode, wParam, lParam);

                if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.ControlKey)
                {
                    _isCtrlDown = (eventType == 256);
                    if (!_isCtrlDown) { _isVDown = false; _copyWindowTimer.Stop(); }
                }

                if (_isCtrlDown && key == Keys.C && eventType == 256)
                {
                    _copyWindowTimer.Restart();
                }

                if (_isCtrlDown && key == Keys.V)
                {
                    if (eventType == 256)
                    {
                        if (!_isVDown) { _isVDown = true; _slotTriggered = false; _vHoldTimer.Restart(); }
                        return (IntPtr)1;
                    }
                    if (eventType == 257)
                    {
                        _isVDown = false;
                        _vHoldTimer.Stop();
                        if (!_slotTriggered && _vHoldTimer.ElapsedMilliseconds < 250) SendPasteSimulated();
                        return (IntPtr)1;
                    }
                }

                if (eventType == 256 && key >= Keys.D1 && key <= Keys.D9)
                {
                    int slotNum = (int)key - (int)Keys.D0;

                    if (_isCtrlDown && _copyWindowTimer.IsRunning && _copyWindowTimer.ElapsedMilliseconds < 500)
                    {
                        _copyWindowTimer.Stop();
                        RunInSTA(() => CaptureToSlot(slotNum));
                        return (IntPtr)1;
                    }

                    if (_isVDown && _vHoldTimer.ElapsedMilliseconds >= 150)
                    {
                        if (_slots.ContainsKey(slotNum))
                        {
                            _slotTriggered = true;
                            RunInSTA(() => PasteFromSlot(slotNum));
                        }
                        return (IntPtr)1;
                    }
                }
            }
            return Win32Interop.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void CaptureToSlot(int slot)
        {
            _clipboardLock.Wait();
            try
            {
                // Give the target app a moment to finish copying to clipboard
                Thread.Sleep(50);
                
                if (Clipboard.ContainsText())
                {
                    _slots[slot] = Clipboard.GetText();
                    SaveToDisk();
                    OSD.Show($"Slot {slot} Saved");
                }
                else
                {
                    // If clipboard is empty or busy, try one more time after a short delay
                    Thread.Sleep(100);
                    if (Clipboard.ContainsText())
                    {
                        _slots[slot] = Clipboard.GetText();
                        SaveToDisk();
                        OSD.Show($"Slot {slot} Saved");
                    }
                }
            }
            catch (Exception ex)
            {
                // Optional: log or handle error
            }
            finally { _clipboardLock.Release(); }
        }

        private void PasteFromSlot(int slot)
        {
            _clipboardLock.Wait();
            try
            {
                if (_slots.ContainsKey(slot))
                {
                    string original = Clipboard.ContainsText() ? Clipboard.GetText() : null;
                    Clipboard.SetText(_slots[slot]);
                    
                    OSD.Show($"Pasting Slot {slot}");
                    
                    SendPasteSimulated();
                    Thread.Sleep(100); // Wait for paste to complete
                    
                    if (original != null) Clipboard.SetText(original);
                }
            }
            catch { }
            finally { _clipboardLock.Release(); }
        }

        private void SaveToDisk()
        {
            try { File.WriteAllText(_savePath, JsonSerializer.Serialize(_slots)); } catch { }
        }

        private void LoadFromDisk()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    var loaded = JsonSerializer.Deserialize<Dictionary<int, string>>(File.ReadAllText(_savePath));
                    if (loaded != null) _slots = loaded;
                }
            }
            catch { }
        }

        private void SendPasteSimulated()
        {
            _suppressNextV = true;
            Win32Interop.keybd_event(0x11, 0, 0, UIntPtr.Zero);
            Win32Interop.keybd_event(0x56, 0, 0, UIntPtr.Zero);
            Win32Interop.keybd_event(0x56, 0, 2, UIntPtr.Zero);
            Win32Interop.keybd_event(0x11, 0, 2, UIntPtr.Zero);
            _suppressNextV = false;
        }

        private void RunInSTA(Action action)
        {
            Thread thread = new Thread(() => action());
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private IntPtr SetHook(Win32Interop.LowLevelKeyboardProc proc)
        {
            using (Process cur = Process.GetCurrentProcess())
            using (ProcessModule mod = cur.MainModule)
            {
                return Win32Interop.SetWindowsHookEx(13, proc, Win32Interop.GetModuleHandle(mod.ModuleName), 0);
            }
        }

        public void Unhook() => Win32Interop.UnhookWindowsHookEx(_hookID);

        public Dictionary<int, string> GetSlots()
        {
            _clipboardLock.Wait();
            try
            {
                return new Dictionary<int, string>(_slots);
            }
            finally
            {
                _clipboardLock.Release();
            }
        }
    }
}