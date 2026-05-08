// ── NAV scroll effect ──
const navbar = document.getElementById('navbar');
window.addEventListener('scroll', () => {
  navbar.style.background = window.scrollY > 40
    ? 'rgba(10,10,15,0.97)'
    : 'rgba(10,10,15,0.85)';
});

// ── Hamburger ──
const hamburger = document.getElementById('hamburger');
const navLinks = document.querySelector('.nav-links');
hamburger.addEventListener('click', () => navLinks.classList.toggle('open'));
navLinks.querySelectorAll('a').forEach(a => a.addEventListener('click', () => navLinks.classList.remove('open')));

// ── Keyboard demo animation ──
const steps = document.querySelectorAll('.demo-step');
let current = 0;
function nextStep() {
  steps[current].classList.remove('active');
  current = (current + 1) % steps.length;
  steps[current].classList.add('active');
}
setInterval(nextStep, 2200);

// ── Scroll-reveal ──
const observer = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      entry.target.style.opacity = '1';
      entry.target.style.transform = 'translateY(0)';
      observer.unobserve(entry.target);
    }
  });
}, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

document.querySelectorAll('.feature-card, .how-card, .arch-layer, .persona-card, .problem-card, .sv-slot').forEach(el => {
  el.style.opacity = '0';
  el.style.transform = 'translateY(24px)';
  el.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
  observer.observe(el);
});

// ── Stagger children ──
document.querySelectorAll('.features-grid, .how-grid, .persona-grid, .sv-grid').forEach(grid => {
  Array.from(grid.children).forEach((child, i) => {
    child.style.transitionDelay = `${i * 80}ms`;
  });
});

// ── Download button click feedback ──
document.querySelectorAll('#hero-download-btn, #main-download-btn').forEach(btn => {
  btn.addEventListener('click', function () {
    this.style.transform = 'scale(0.97)';
    setTimeout(() => this.style.transform = '', 150);
  });
});

// ── Slot hover tooltip ──
document.querySelectorAll('.sv-slot.filled').forEach(slot => {
  slot.title = `Slot ${slot.dataset.slot} — Click V + ${slot.dataset.slot} to paste`;
});
