# SlotCopy

**A high-performance, slot-based clipboard manager for Windows built for speed, muscle memory, and zero context switching.**

## Introduction

SlotCopy is a sophisticated Windows utility that operates at the Kernel-to-User interface, redefining how power users handle copy-pasting. Moving away from the traditional "history-based" clipboard model, SlotCopy implements a **"Slot Model"** that allows users to bind specific text snippets to dedicated hardware keys. 

By leveraging human spatial memory instead of visual lists, SlotCopy functions seamlessly in the background as an invisible "utility layer," empowering developers, researchers, and data entry professionals to work at the speed of thought.

---

## The Problem It Solves

**The Multiple-Block Overhead:** Consider the scenario where you need to copy multiple separate blocks of text from one webpage to another document. Traditionally, you must: copy block 1 -> switch windows -> paste -> switch back -> locate block 2 -> copy -> repeat. The continuous window switching and the distraction of constantly re-finding your place on the source page is a massive workflow bottleneck.

Standard clipboard managers—including native OS solutions like `Win+V`—rely heavily on the **History Model**. Every time you copy something new, your previous items shift down a visual list. Retrieving an older item requires you to:
1. Trigger the clipboard UI.
2. Break your visual focus from your current task.
3. Scroll and visually scan for the correct snippet.
4. Select and paste.

This process induces a **Context Switch**. Studies in productivity show that even minor context switches drastically increase cognitive load and drain mental energy, breaking the user's "Flow State."

**The Multiple-Block Overhead:** Consider the scenario where you need to copy multiple separate blocks of text from one webpage to another document. Traditionally, you must: copy block 1 -> switch windows -> paste -> switch back -> locate block 2 -> copy -> repeat. The continuous window switching and the distraction of constantly re-finding your place on the source page is a massive workflow bottleneck.

**SlotCopy eliminates these bottlenecks.** 
- **No Visual Search:** If you assign your database connection string to Slot 1, it will *always* be Slot 1. You don't look for it; your fingers just know where it is. 
- **Batch Copying:** You can stay on your source page, copy multiple blocks of text one after another into slots 1 through 9, and then switch to your target document just *once* to paste them sequentially. 
- **Protected Main Memory:** Standard copy/paste continues to work seamlessly alongside your slots, meaning you never disturb your primary OS clipboard memory. 

By minimizing the friction of data movement to a sheer hardware-level shortcut, SlotCopy radically reduces cognitive load and preserves your focus.

---

## How to Use

SlotCopy relies on timing-based logic gates rather than complex multi-key combinations, ensuring your existing keyboard habits remain completely uninterrupted. 

### Saving to a Slot
1. Highlight your desired text and press `Ctrl + C` (Normal Copy).
2. Within **500 milliseconds**, press a number key from `1` to `9`.
3. The snippet is now bound to that slot. (An On-Screen Display will confirm the save).
   * *Note: If you don't press a number within 500ms, the number keys behave normally.*

### Pasting from a Slot (The "Two-Tap" Advantage)
1. **Hold** the `V` key down.
2. Press the corresponding number key (`1` - `9`) for your desired slot.
3. Release `V`. The snippet is instantly pasted.

### Standard Pasting
- To execute a normal paste (`Ctrl + V`), simply tap the `V` key rapidly (under 250 milliseconds). SlotCopy will intelligently recognize the tap and pass the standard paste command through to the OS.

---

## Technical Architecture & Backend Implementation

SlotCopy is engineered for absolute zero-latency. To achieve its "invisible" UX, it bypasses standard application-layer limitations and splits its architecture into four distinct, highly optimized layers: `Win32Interop` (OS Bridge), `GlobalKeyboardHook` (Logic Brain), `TrayContext` (Lifecycle), and `OSD` (Feedback).

### 1. The Low-Level Keyboard Hook (`WH_KEYBOARD_LL`)
At the core of the application is a system-wide Windows Hook (`SetWindowsHookEx` with ID 13). 
- **Pre-Emptive Interception:** Every keystroke is intercepted and inspected before it reaches any target application.
- **Microsecond Bail-Out:** To prevent systemic input lag, the application features an immediate bail-out mechanism. Irrelevant keys are passed down the chain via `CallNextHookEx` in nanoseconds, ensuring the hook "steps out of the way" instantly.

### 2. State Machine & Logic Gates
Instead of standard global hotkeys, SlotCopy uses precise timing-based state machines. 
- **The Ctrl+C Window:** Triggers a strict 500ms `Stopwatch` thread, listening for numeric assignment.
- **The V-Hold Gate:** Implements sophisticated "Swallow" logic. Upon key-down, the `V` key is blocked at the kernel level (`return (IntPtr)1`). If tapped quickly, a simulated paste is fired. If held while a number is pressed, the app executes a Slot Paste and permanently suppresses the original keypress.

### 3. Clipboard Synchronization & STA Marshalling
Windows Clipboards are notoriously volatile and strictly require Single Threaded Apartment (STA) access.
- **Thread Marshalling:** Because the keyboard hook runs on a system thread, all clipboard interactions are marshaled to a dedicated background STA thread via `RunInSTA`.
- **The Snapshot Pattern:** To paste from a slot without overwriting the user's actual clipboard history, SlotCopy executes a highly controlled sequence: 
  1. Snapshots and saves the current clipboard data.
  2. Injects the selected Slot data into the clipboard.
  3. Simulates a `Ctrl+V` keystroke.
  4. Waits 75ms for the target application's message pump to read the payload.
  5. Restores the original snapshot to the clipboard.

### 4. Thread-Safe UI & Performance Optimizations
- **SynchronizationContext:** Background logic threads safely push On-Screen Display (OSD) rendering instructions to the Main UI Thread's message loop, preventing cross-thread exceptions.
- **OLE Overhead Bypass:** Data is stored exclusively as raw Strings rather than `DataObject` instances, resulting in instantaneous read/write speeds.
- **Smart Persistence:** Slots are serialized via `System.Text.Json` to a compact `slots.json` file, executing Disk I/O *only* upon state changes.
- **Memory Protection:** Static references are utilized to prevent the .NET Garbage Collector from aggressively cleaning up system hooks while running in the background.
