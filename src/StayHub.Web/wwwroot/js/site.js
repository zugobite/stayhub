// StayHub - client-side helpers.
// Progressive enhancement only: every interaction below has a working
// server-side fallback so the app still functions with scripting disabled.

// --- Password show / hide toggle --------------------------------------------
(function () {
    'use strict';

    document.querySelectorAll('[data-sh-password-toggle]').forEach(function (button) {
        var field = button.closest('.sh-password-field');

        if (!field) {
            return;
        }

        var input = field.querySelector('input');

        if (!input) {
            return;
        }

        var showIcon = button.querySelector('[data-sh-password-icon="show"]');
        var hideIcon = button.querySelector('[data-sh-password-icon="hide"]');

        button.addEventListener('click', function () {
            var visible = input.type === 'text';
            input.type = visible ? 'password' : 'text';
            button.setAttribute('aria-pressed', visible ? 'false' : 'true');
            button.setAttribute('aria-label', visible ? 'Show password' : 'Hide password');

            if (showIcon) {
                showIcon.hidden = !visible;
            }

            if (hideIcon) {
                hideIcon.hidden = visible;
            }
        });
    });
})();

// --- Live price breakdown on the property booking widget ---------------------
(function () {
    'use strict';

    var breakdown = document.querySelector('.sh-pricebreakdown');
    var checkIn = document.getElementById('checkIn');
    var checkOut = document.getElementById('checkOut');
    var guestsInput = document.getElementById('guests');

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

        // The nightly rate is per person, so party size scales the subtotal and,
        // through it, the service fee. The cleaning fee stays flat per stay.
        var guests = parseInt(guestsInput && guestsInput.value, 10);

        if (!isFinite(guests) || guests < 1) {
            guests = 1;
        }

        var subtotal = nightly * nights * guests;
        var serviceFee = Math.round(subtotal * serviceRate * 100) / 100;
        var total = subtotal + cleaning + serviceFee;

        document.getElementById('nightsLabel').textContent =
            'R ' + rand.format(nightly) + ' × ' + nights + (nights === 1 ? ' night' : ' nights') +
            ' × ' + guests + (guests === 1 ? ' guest' : ' guests');
        document.getElementById('subtotal').textContent = 'R ' + rand.format(subtotal);
        document.getElementById('serviceFee').textContent = 'R ' + rand.format(serviceFee);
        document.getElementById('total').textContent = 'R ' + rand.format(total);
    }

    checkIn.addEventListener('change', recalculate);
    checkOut.addEventListener('change', recalculate);

    if (guestsInput) {
        guestsInput.addEventListener('change', recalculate);
        guestsInput.addEventListener('input', recalculate);
    }
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

// --- Listing gallery pagination + fullscreen lightbox -----------------------
// The track already scrolls and snaps by itself; arrows/dots/counter stay in
// step with it. The magnifying glass opens the current photo full screen.
(function () {
    'use strict';

    var lightbox = null;
    var lightboxImg = null;
    var lightboxCounter = null;
    var lightboxPrev = null;
    var lightboxNext = null;
    var lightboxSources = [];
    var lightboxIndex = 0;
    var lastFocus = null;

    function ensureLightbox() {
        if (lightbox) {
            return;
        }

        lightbox = document.createElement('div');
        lightbox.className = 'sh-lightbox';
        lightbox.hidden = true;
        lightbox.setAttribute('role', 'dialog');
        lightbox.setAttribute('aria-modal', 'true');
        lightbox.setAttribute('aria-label', 'Full screen photo');
        lightbox.innerHTML =
            '<button class="sh-lightbox-close" type="button" data-sh-lightbox-close aria-label="Close full screen photo">' +
                '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" aria-hidden="true">' +
                    '<path d="M18 6 6 18M6 6l12 12"/>' +
                '</svg>' +
            '</button>' +
            '<button class="sh-lightbox-nav prev" type="button" data-sh-lightbox-prev aria-label="Previous photo">' +
                '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' +
                    '<path d="M15 5l-7 7 7 7"/>' +
                '</svg>' +
            '</button>' +
            '<img class="sh-lightbox-image" alt="" data-sh-lightbox-image />' +
            '<button class="sh-lightbox-nav next" type="button" data-sh-lightbox-next aria-label="Next photo">' +
                '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' +
                    '<path d="M9 5l7 7-7 7"/>' +
                '</svg>' +
            '</button>' +
            '<span class="sh-lightbox-counter" data-sh-lightbox-counter aria-live="polite"></span>';

        document.body.appendChild(lightbox);

        lightboxImg = lightbox.querySelector('[data-sh-lightbox-image]');
        lightboxCounter = lightbox.querySelector('[data-sh-lightbox-counter]');
        lightboxPrev = lightbox.querySelector('[data-sh-lightbox-prev]');
        lightboxNext = lightbox.querySelector('[data-sh-lightbox-next]');

        lightbox.addEventListener('click', function (event) {
            if (event.target === lightbox || event.target.closest('[data-sh-lightbox-close]')) {
                closeLightbox();
            }
        });

        lightboxPrev.addEventListener('click', function (event) {
            event.stopPropagation();
            showLightboxIndex(lightboxIndex - 1);
        });

        lightboxNext.addEventListener('click', function (event) {
            event.stopPropagation();
            showLightboxIndex(lightboxIndex + 1);
        });

        document.addEventListener('keydown', function (event) {
            if (lightbox.hidden) {
                return;
            }

            if (event.key === 'Escape') {
                event.preventDefault();
                closeLightbox();
            } else if (event.key === 'ArrowLeft') {
                event.preventDefault();
                showLightboxIndex(lightboxIndex - 1);
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                showLightboxIndex(lightboxIndex + 1);
            }
        });
    }

    function showLightboxIndex(index) {
        if (!lightboxSources.length) {
            return;
        }

        lightboxIndex = (index + lightboxSources.length) % lightboxSources.length;
        var item = lightboxSources[lightboxIndex];
        lightboxImg.src = item.src;
        lightboxImg.alt = item.alt || 'Listing photo';
        lightboxCounter.textContent = (lightboxIndex + 1) + ' / ' + lightboxSources.length;

        var multi = lightboxSources.length > 1;
        lightboxPrev.hidden = !multi;
        lightboxNext.hidden = !multi;
        lightboxCounter.hidden = !multi;
    }

    function openLightbox(sources, index) {
        ensureLightbox();
        lightboxSources = sources;
        lastFocus = document.activeElement;
        showLightboxIndex(index || 0);
        lightbox.hidden = false;
        document.body.classList.add('sh-lightbox-open');
        var closeBtn = lightbox.querySelector('[data-sh-lightbox-close]');
        if (closeBtn) {
            closeBtn.focus();
        }
    }

    function closeLightbox() {
        if (!lightbox || lightbox.hidden) {
            return;
        }

        lightbox.hidden = true;
        document.body.classList.remove('sh-lightbox-open');
        lightboxImg.removeAttribute('src');
        lightboxSources = [];

        if (lastFocus && typeof lastFocus.focus === 'function') {
            lastFocus.focus();
        }
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-sh-gallery]'), function (gallery) {
        var track = gallery.querySelector('[data-sh-gallery-track]');

        if (!track || !track.children.length) {
            return;
        }

        var slides = track.children;
        var prev = gallery.querySelector('[data-sh-gallery-prev]');
        var next = gallery.querySelector('[data-sh-gallery-next]');
        var counter = gallery.querySelector('[data-sh-gallery-counter]');
        var dots = gallery.querySelectorAll('[data-sh-gallery-dot]');
        var zoom = gallery.querySelector('[data-sh-gallery-zoom]');
        var ticking = false;
        var multi = slides.length > 1;

        function currentIndex() {
            if (!multi || !track.clientWidth) {
                return 0;
            }

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

        function gallerySources() {
            return Array.prototype.map.call(slides, function (slide) {
                return {
                    src: slide.getAttribute('data-sh-gallery-src') || slide.currentSrc || slide.src,
                    alt: slide.getAttribute('alt') || ''
                };
            });
        }

        function openCurrent() {
            openLightbox(gallerySources(), currentIndex());
        }

        if (zoom) {
            zoom.addEventListener('click', function (event) {
                event.preventDefault();
                openCurrent();
            });
        }

        Array.prototype.forEach.call(slides, function (slide, index) {
            slide.style.cursor = 'zoom-in';
            slide.addEventListener('click', function () {
                openLightbox(gallerySources(), index);
            });
        });

        if (!multi) {
            return;
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

        window.addEventListener('resize', sync);
        sync();
    });
})();

// --- File drop-zone with preview thumbnails ---------------------------------
(function () {
    'use strict';

    var removeIcon =
        '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" aria-hidden="true">' +
            '<path d="M18 6 6 18M6 6l12 12"/>' +
        '</svg>';

    function acceptsFile(input, file) {
        var accept = (input.getAttribute('accept') || '').trim();

        if (!accept) {
            return true;
        }

        return accept.split(',').some(function (token) {
            token = token.trim().toLowerCase();

            if (!token) {
                return false;
            }

            if (token === 'image/*') {
                return (file.type || '').indexOf('image/') === 0;
            }

            if (token.charAt(0) === '.') {
                var name = (file.name || '').toLowerCase();
                return name.slice(-token.length) === token;
            }

            return (file.type || '').toLowerCase() === token;
        });
    }

    function syncInput(input, files) {
        var transfer = new DataTransfer();

        files.forEach(function (file) {
            transfer.items.add(file);
        });

        input.files = transfer.files;
    }

    function initDropzone(root) {
        var input = root.querySelector('[data-sh-dropzone-input]');
        var surface = root.querySelector('[data-sh-dropzone-surface]');
        var trigger = root.querySelector('[data-sh-dropzone-trigger]');
        var list = root.querySelector('[data-sh-dropzone-previews]');

        if (!input || !surface || !list) {
            return;
        }

        var selected = [];
        var objectUrls = [];
        var dragDepth = 0;
        var multiple = input.hasAttribute('multiple');

        function clearObjectUrls() {
            objectUrls.forEach(function (url) {
                URL.revokeObjectURL(url);
            });
            objectUrls = [];
        }

        function render() {
            clearObjectUrls();
            list.innerHTML = '';

            if (!selected.length) {
                list.hidden = true;
                return;
            }

            list.hidden = false;

            selected.forEach(function (file, index) {
                var item = document.createElement('li');
                var isImage = (file.type || '').indexOf('image/') === 0;
                var remove = document.createElement('button');

                remove.type = 'button';
                remove.className = 'sh-dropzone-remove';
                remove.setAttribute('aria-label', 'Remove ' + (file.name || 'file'));
                remove.innerHTML = removeIcon;
                remove.addEventListener('click', function (event) {
                    event.preventDefault();
                    event.stopPropagation();
                    selected.splice(index, 1);
                    syncInput(input, selected);
                    render();
                });

                if (isImage) {
                    var img = document.createElement('img');
                    var url = URL.createObjectURL(file);
                    objectUrls.push(url);
                    img.src = url;
                    img.alt = '';
                    item.className = 'sh-dropzone-preview';
                    item.appendChild(img);
                } else {
                    var name = document.createElement('span');
                    name.className = 'sh-dropzone-docname';
                    name.textContent = file.name || 'Document';
                    item.className = 'sh-dropzone-preview is-document';
                    item.appendChild(name);
                }

                item.appendChild(remove);
                list.appendChild(item);
            });
        }

        function applyFiles(fileList) {
            var incoming = Array.prototype.slice.call(fileList || []).filter(function (file) {
                return acceptsFile(input, file);
            });

            if (!incoming.length) {
                return;
            }

            selected = multiple ? selected.concat(incoming) : incoming.slice(0, 1);
            syncInput(input, selected);
            render();
        }

        function openPicker(event) {
            if (event) {
                event.preventDefault();
            }

            input.click();
        }

        if (trigger) {
            trigger.addEventListener('click', function (event) {
                event.preventDefault();
                event.stopPropagation();
                openPicker();
            });
        }

        surface.addEventListener('click', function (event) {
            if (event.target.closest('[data-sh-dropzone-trigger]')) {
                return;
            }

            openPicker(event);
        });

        surface.addEventListener('dragenter', function (event) {
            event.preventDefault();
            dragDepth += 1;
            root.classList.add('is-dragging');
        });

        surface.addEventListener('dragover', function (event) {
            event.preventDefault();
            if (event.dataTransfer) {
                event.dataTransfer.dropEffect = 'copy';
            }
        });

        surface.addEventListener('dragleave', function (event) {
            event.preventDefault();
            dragDepth = Math.max(0, dragDepth - 1);

            if (dragDepth === 0) {
                root.classList.remove('is-dragging');
            }
        });

        surface.addEventListener('drop', function (event) {
            event.preventDefault();
            dragDepth = 0;
            root.classList.remove('is-dragging');
            applyFiles(event.dataTransfer && event.dataTransfer.files);
        });

        input.addEventListener('change', function () {
            applyFiles(input.files);
            syncInput(input, selected);
        });
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-sh-dropzone]'), initDropzone);
})();
