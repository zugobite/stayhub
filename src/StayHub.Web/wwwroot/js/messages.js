// StayHub - live conversation send/receive on Messages/Conversation.
(function () {
    'use strict';

    var thread = document.getElementById('shThread');
    var form = document.getElementById('shReplyForm');

    if (!thread || !form) {
        return;
    }

    var emptyState = thread.querySelector('.sh-empty');

    function scrollToBottom() {
        thread.scrollTop = thread.scrollHeight;
        window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
    }

    function escapeHtml(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    window.shAppendConversationMessage = function (payload, mine) {
        if (!payload || !payload.id) {
            return;
        }

        if (document.querySelector('[data-message-id="' + payload.id + '"]')) {
            return;
        }

        if (emptyState) {
            emptyState.remove();
            emptyState = null;
        }

        var bubble = document.createElement('div');
        bubble.className = 'sh-bubble ' + (mine ? 'mine' : 'theirs');
        bubble.setAttribute('data-message-id', payload.id);
        bubble.innerHTML =
            '<div>' + escapeHtml(payload.content) + '</div>' +
            '<div class="sh-bubble-time">' + escapeHtml(payload.displayTime || '') + '</div>';

        thread.appendChild(bubble);
        scrollToBottom();
    };

    form.addEventListener('submit', function (event) {
        event.preventDefault();

        var textarea = form.querySelector('textarea[name="content"]');
        var content = textarea && textarea.value.trim();

        if (!content) {
            return;
        }

        var body = new FormData(form);
        var button = form.querySelector('button[type="submit"]');

        if (button) {
            button.disabled = true;
        }

        fetch(form.action, {
            method: 'POST',
            body: body,
            headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Send failed');
                }

                return response.json();
            })
            .then(function (payload) {
                window.shAppendConversationMessage(payload, true);
                textarea.value = '';
            })
            .catch(function () {
                // Fall back to a classic post so the message is not lost.
                form.removeAttribute('data-ajax');
                form.querySelector('input[name="ajax"]').value = 'false';
                form.submit();
            })
            .finally(function () {
                if (button) {
                    button.disabled = false;
                }
            });
    });

    scrollToBottom();
})();
