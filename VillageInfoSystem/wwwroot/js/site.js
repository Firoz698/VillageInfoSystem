/* ═══════════════════════════════════════════════════════════
   VILLAGE INFO SYSTEM — PUBLIC INTERACTIVE SCRIPTS
═══════════════════════════════════════════════════════════ */

let cur = 0;
const DURATION = 6000;
let autoTimer, progTimer, progW = 0;

document.addEventListener('DOMContentLoaded', function () {
    const slides = document.querySelectorAll('.slide');
    const dots = document.querySelectorAll('.dot');
    const progBar = document.getElementById('sProgress');
    const curNumEl = document.getElementById('curNum');
    const TOTAL = slides.length;

    /* ── HERO SLIDER ── */
    if (TOTAL > 0) {
        window.showSlide = function (idx) {
            slides.forEach(s => s.classList.remove('active'));
            dots.forEach(d => d.classList.remove('active'));
            if (slides[idx]) slides[idx].classList.add('active');
            if (dots[idx]) dots[idx].classList.add('active');
            if (curNumEl) curNumEl.textContent = idx + 1;
        };

        window.changeSlide = function (dir) {
            cur = (cur + dir + TOTAL) % TOTAL;
            showSlide(cur);
            resetAuto();
        };

        window.goTo = function (idx) {
            cur = idx;
            showSlide(cur);
            resetAuto();
        };

        function startAuto() {
            clearInterval(autoTimer);
            clearInterval(progTimer);
            progW = 0;
            if (progBar) progBar.style.width = '0%';
            
            progTimer = setInterval(() => {
                progW += 100 / (DURATION / 80);
                if (progBar) progBar.style.width = Math.min(progW, 100) + '%';
            }, 80);

            autoTimer = setInterval(() => {
                cur = (cur + 1) % TOTAL;
                showSlide(cur);
                progW = 0;
            }, DURATION);
        }

        function resetAuto() {
            progW = 0;
            if (progBar) progBar.style.width = '0%';
            startAuto();
        }

        // Swipe support on Mobile
        let touchStartX = 0;
        const heroEl = document.getElementById('home');
        if (heroEl) {
            heroEl.addEventListener('touchstart', e => {
                touchStartX = e.touches[0].clientX;
            }, { passive: true });

            heroEl.addEventListener('touchend', e => {
                const diff = touchStartX - e.changedTouches[0].clientX;
                if (Math.abs(diff) > 40) {
                    changeSlide(diff > 0 ? 1 : -1);
                }
            });
        }

        document.addEventListener('keydown', e => {
            if (e.key === 'ArrowLeft') changeSlide(-1);
            if (e.key === 'ArrowRight') changeSlide(1);
        });

        showSlide(0);
        startAuto();
    }

    /* ── MOBILE MENU ── */
    window.toggleMenu = function () {
        const m = document.getElementById('mobileMenu');
        if (m) m.classList.toggle('open');
    };

    document.addEventListener('click', e => {
        const m = document.getElementById('mobileMenu');
        const h = document.querySelector('.hamburger');
        if (m && h && !m.contains(e.target) && !h.contains(e.target)) {
            m.classList.remove('open');
        }
    });

    /* ── SCROLL REVEAL ANIMATIONS ── */
    const reveals = document.querySelectorAll('.reveal');
    if ('IntersectionObserver' in window) {
        const revealObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.12,
            rootMargin: '0px 0px -40px 0px'
        });

        reveals.forEach(el => revealObserver.observe(el));
    } else {
        reveals.forEach(el => el.classList.add('visible'));
    }

    /* ── ACTIVE NAV HIGHLIGHT ON SCROLL ── */
    const sections = document.querySelectorAll('section[id], div#home');
    const navLinks = document.querySelectorAll('.nav-links a');

    window.addEventListener('scroll', () => {
        let currentSectionId = '';
        const scrollPosition = window.scrollY + 140;

        sections.forEach(sec => {
            if (scrollPosition >= sec.offsetTop) {
                currentSectionId = sec.getAttribute('id');
            }
        });

        navLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href === '#' + currentSectionId) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });

        // Scroll-to-top button
        const scrollTopBtn = document.getElementById('scrollTop');
        if (scrollTopBtn) {
            scrollTopBtn.classList.toggle('visible', window.scrollY > 450);
        }
    }, { passive: true });

    /* ── NUMBER COUNTER ANIMATION ── */
    const counters = document.querySelectorAll('.info-num, .stat-num');
    if ('IntersectionObserver' in window) {
        const counterObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const el = entry.target;
                    const text = el.textContent.trim();
                    const numMatch = text.match(/[\d,]+/);
                    if (numMatch) {
                        const target = parseInt(numMatch[0].replace(/,/g, ''), 10);
                        if (!isNaN(target) && target > 0) {
                            animateNumber(el, target, text);
                        }
                    }
                    observer.unobserve(el);
                }
            });
        }, { threshold: 0.5 });

        counters.forEach(c => counterObserver.observe(c));
    }

    function animateNumber(element, target, originalText) {
        const duration = 1200;
        const start = 0;
        const startTime = performance.now();

        function update(now) {
            const elapsed = now - startTime;
            const progress = Math.min(elapsed / duration, 1);
            // Ease out cubic
            const easeProgress = 1 - Math.pow(1 - progress, 3);
            const current = Math.floor(start + (target - start) * easeProgress);

            element.textContent = originalText.replace(/[\d,]+/, current.toLocaleString());

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                element.textContent = originalText;
            }
        }

        requestAnimationFrame(update);
    }
});
