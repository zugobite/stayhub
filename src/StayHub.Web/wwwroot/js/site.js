// StayHub - client-side helpers.
// Progressive enhancement only: every interaction below has a working
// server-side fallback so the app still functions with scripting disabled.

// --- Live price breakdown on the property booking widget ---------------------
(function () {
    'use strict';

    var breakdown = document.querySelector('.sh-pricebreakdown');
    var checkIn = document.getElementById('checkIn');
    var checkOut = document.getElementById('checkOut');

    if (!breakdown || !checkIn || !checkOut) {
        return;
    }

    var nightly = parseFloat(breakdown.dataset.nightly) || 0;
    var cleaning = parseFloat(breakdown.dataset.cleaning) || 0;
    var serviceRate = parseFloat(breakdown.dataset.servicerate) || 0;

    var rand = new Intl.NumberFormat('en-ZA', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    /** Recalculates the breakdown to match the server-side pricing formula. */
    function recalculate() {
        var start = new Date(checkIn.value);
        var end = new Date(checkOut.value);
        var nights = Math.round((end - start) / 86400000);

        if (!isFinite(nights) || nights < 1) {
            nights = 1;
        }

        var subtotal = nightly * nights;
        var serviceFee = Math.round(subtotal * serviceRate * 100) / 100;
        var total = subtotal + cleaning + serviceFee;

        document.getElementById('nightsLabel').textContent =
            'R ' + rand.format(nightly) + ' × ' + nights + (nights === 1 ? ' night' : ' nights');
        document.getElementById('subtotal').textContent = 'R ' + rand.format(subtotal);
        document.getElementById('serviceFee').textContent = 'R ' + rand.format(serviceFee);
        document.getElementById('total').textContent = 'R ' + rand.format(total);
    }

    checkIn.addEventListener('change', recalculate);
    checkOut.addEventListener('change', recalculate);
    recalculate();
})();

// --- Branded confirmation dialog --------------------------------------------
// Replaces window.confirm() for destructive actions. Forms opt in with
// data-sh-confirm="message" and, optionally, data-sh-confirm-title /
// data-sh-confirm-action to relabel the dialog.
(function () {
    'use strict';

    var element = document.getElementById('shConfirm');

    if (!element || typeof bootstrap === 'undefined') {
        return;
    }

    var dialog = new bootstrap.Modal(element);
    var message = document.getElementById('shConfirmMessage');
    var title = document.getElementById('shConfirmTitle');
    var accept = document.getElementById('shConfirmAccept');
    var pending = null;

    document.addEventListener('submit', function (event) {
        var form = event.target.closest('form[data-sh-confirm]');

        if (!form || form.dataset.shConfirmed === 'true') {
            return;
        }

        event.preventDefault();
        pending = form;

        message.textContent = form.dataset.shConfirm;
        title.textContent = form.dataset.shConfirmTitle || 'Please confirm';
        accept.textContent = form.dataset.shConfirmAction || 'Confirm';
        accept.classList.toggle('sh-btn-danger', form.dataset.shConfirmDanger === 'true');
        accept.classList.toggle('sh-btn-primary', form.dataset.shConfirmDanger !== 'true');

        dialog.show();
    });

    accept.addEventListener('click', function () {
        if (!pending) {
            return;
        }

        // Mark the form so the submit handler lets the second attempt through.
        pending.dataset.shConfirmed = 'true';
        dialog.hide();
        pending.requestSubmit ? pending.requestSubmit() : pending.submit();
        pending = null;
    });

    element.addEventListener('hidden.bs.modal', function () {
        pending = null;
    });
})();

// --- Listing gallery pagination ---------------------------------------------
// The track already scrolls and snaps by itself; this only keeps the arrows,
// dots and counter in step with it.
(function () {
    'use strict';

    Array.prototype.forEach.call(document.querySelectorAll('[data-sh-gallery]'), function (gallery) {
        var track = gallery.querySelector('[data-sh-gallery-track]');

        if (!track || track.children.length < 2) {
            return;
        }

        var slides = track.children;
        var prev = gallery.querySelector('[data-sh-gallery-prev]');
        var next = gallery.querySelector('[data-sh-gallery-next]');
        var counter = gallery.querySelector('[data-sh-gallery-counter]');
        var dots = gallery.querySelectorAll('[data-sh-gallery-dot]');
        var ticking = false;

        function currentIndex() {
            return Math.round(track.scrollLeft / track.clientWidth);
        }

        function goTo(index) {
            var target = Math.max(0, Math.min(index, slides.length - 1));
            track.scrollLeft = target * track.clientWidth;
        }

        function sync() {
            var index = currentIndex();

            if (counter) {
                counter.textContent = (index + 1) + ' / ' + slides.length;
            }

            Array.prototype.forEach.call(dots, function (dot, i) {
                if (i === index) {
                    dot.setAttribute('aria-current', 'true');
                } else {
                    dot.removeAttribute('aria-current');
                }
            });

            if (prev) {
                prev.disabled = index === 0;
            }

            if (next) {
                next.disabled = index === slides.length - 1;
            }
        }

        if (prev) {
            prev.addEventListener('click', function () {
                goTo(currentIndex() - 1);
            });
        }

        if (next) {
            next.addEventListener('click', function () {
                goTo(currentIndex() + 1);
            });
        }

        Array.prototype.forEach.call(dots, function (dot) {
            dot.addEventListener('click', function () {
                goTo(parseInt(dot.dataset.shGalleryDot, 10));
            });
        });

        track.addEventListener('keydown', function (event) {
            if (event.key === 'ArrowLeft') {
                event.preventDefault();
                goTo(currentIndex() - 1);
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                goTo(currentIndex() + 1);
            }
        });

        track.addEventListener('scroll', function () {
            if (ticking) {
                return;
            }

            ticking = true;
            window.requestAnimationFrame(function () {
                ticking = false;
                sync();
            });
        });

        // Keep the snap position valid when the track is resized.
        window.addEventListener('resize', sync);

        sync();
    });
})();