/* HERO SLIDER */
let cur = 0;
const TOTAL = 5;
const DURATION = 5000;
let autoTimer, progTimer, progW = 0;

document.addEventListener('DOMContentLoaded', function () {
    const slides = document.querySelectorAll('.slide');
    const dots = document.querySelectorAll('.dot');
    const progBar = document.getElementById('sProgress');
    const curNumEl = document.getElementById('curNum');

    if (!slides.length) return;

    window.showSlide = function (idx) {
        slides.forEach(s => s.classList.remove('active'));
        dots.forEach(d => d.classList.remove('active'));
        slides[idx].classList.add('active');
        if (dots[idx]) dots[idx].classList.add('active');
        if (curNumEl) curNumEl.textContent = idx + 1;
    };

    window.changeSlide = function (dir) {
        cur = (cur + dir + TOTAL) % TOTAL;
        showSlide(cur); resetAuto();
    };

    window.goTo = function (idx) {
        cur = idx; showSlide(cur); resetAuto();
    };

    function startAuto() {
        clearInterval(autoTimer); clearInterval(progTimer);
        progW = 0; if (progBar) progBar.style.width = '0%';
        progTimer = setInterval(() => {
            progW += 100 / (DURATION / 100);
            if (progBar) progBar.style.width = Math.min(progW, 100) + '%';
        }, 100);
        autoTimer = setInterval(() => {
            cur = (cur + 1) % TOTAL;
            showSlide(cur); progW = 0;
        }, DURATION);
    }

    function resetAuto() {
        progW = 0; if (progBar) progBar.style.width = '0%'; startAuto();
    }

    // Touch
    let tx = 0;
    const heroEl = document.getElementById('home');
    if (heroEl) {
        heroEl.addEventListener('touchstart', e => tx = e.touches[0].clientX, { passive: true });
        heroEl.addEventListener('touchend', e => {
            const d = tx - e.changedTouches[0].clientX;
            if (Math.abs(d) > 45) changeSlide(d > 0 ? 1 : -1);
        });
    }

    document.addEventListener('keydown', e => {
        if (e.key === 'ArrowLeft') changeSlide(-1);
        if (e.key === 'ArrowRight') changeSlide(1);
    });

    showSlide(0); startAuto();

    /* MOBILE MENU */
    window.toggleMenu = function () {
        document.getElementById('mobileMenu').classList.toggle('open');
    };
    document.addEventListener('click', e => {
        const m = document.getElementById('mobileMenu');
        const h = document.querySelector('.hamburger');
        if (m && h && !m.contains(e.target) && !h.contains(e.target)) m.classList.remove('open');
    });

    /* SCROLL REVEAL */
    const reveals = document.querySelectorAll('.reveal');
    reveals.forEach(el => {
        new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) entry.target.classList.add('visible');
            });
        }, { threshold: 0.1 }).observe(el);
    });

    /* ACTIVE NAV */
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-links a');
    window.addEventListener('scroll', () => {
        let cur2 = '';
        sections.forEach(s => { if (window.scrollY >= s.offsetTop - 130) cur2 = s.id; });
        navLinks.forEach(a => {
            const on = a.getAttribute('href') === '#' + cur2;
            a.style.color = on ? 'var(--gold-light)' : '';
            a.style.background = on ? 'rgba(200,153,58,0.15)' : '';
        });
    });
});
