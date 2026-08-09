// StayHub - live SignalR client for notification and message badges.
(function () {
    'use strict';

    if (typeof signalR === 'undefined') {
        return;
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/stayhub')
        .withAutomaticReconnect()
        .build();

    function setBadge(id, count) {
        var badge = document.getElementById(id);

        if (!badge) {
            return;
        }

        if (count > 0) {
            badge.textContent = String(count);
            badge.hidden = false;
        } else {
            badge.textContent = '0';
            badge.hidden = true;
        }
    }

    function showToast(title, body, actionUrl) {
        var host = document.getElementById('shToastHost');

        if (!host) {
            return;
        }

        var toast = document.createElement('a');
        toast.className = 'sh-toast';
        toast.href = actionUrl || '/Notifications';
        toast.innerHTML =
            '<strong>' + escapeHtml(title) + '</strong>' +
            '<span>' + escapeHtml(body) + '</span>';

        host.appendChild(toast);

        window.setTimeout(function () {
            toast.classList.add('is-leaving');
            window.setTimeout(function () {
                toast.remove();
            }, 250);
        }, 5000);
    }

    function escapeHtml(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    connection.on('notificationReceived', function (payload) {
        if (payload && typeof payload.unreadCount === 'number') {
            setBadge('shNotifBadge', payload.unreadCount);
        }

        if (payload && payload.title) {
            showToast(payload.title, payload.body || '', payload.actionUrl);
        }
    });

    connection.on('messageBadgeUpdated', function (payload) {
        if (payload && typeof payload.unreadCount === 'number') {
            setBadge('shMessageBadge', payload.unreadCount);
        }
    });

    connection.on('messageReceived', function (payload) {
        // Conversation page owns bubble rendering; elsewhere bump the badge
        // if the message is incoming (receiver is viewing another page).
        if (!payload) {
            return;
        }

        var thread = document.getElementById('shThread');
        var currentUserId = thread && thread.dataset.currentUserId;

        if (!thread && typeof payload.receiverId === 'string') {
            // Outside the conversation - badge is updated via messageBadgeUpdated.
            return;
        }

        if (thread &&
            typeof window.shAppendConversationMessage === 'function' &&
            payload.senderId === thread.dataset.conversationUserId) {
            window.shAppendConversationMessage(payload, false);
        } else if (thread &&
            currentUserId &&
            payload.senderId === currentUserId &&
            typeof window.shAppendConversationMessage === 'function') {
            // Multi-tab echo for the sender - skip if already rendered via AJAX.
            if (!document.querySelector('[data-message-id="' + payload.id + '"]')) {
                window.shAppendConversationMessage(payload, true);
            }
        }
    });

    connection.start().catch(function () {
        // Silent: messaging still works via full-page POST without live push.
    });

    window.shStayHubConnection = connection;
})();
