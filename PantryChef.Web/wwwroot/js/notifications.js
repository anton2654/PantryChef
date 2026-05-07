(function () {
  document.addEventListener('DOMContentLoaded', function () {
    const list = document.getElementById('notification-list');
    const count = document.getElementById('notification-count');

    if (!list || !count) {
      return;
    }

    const token = document.querySelector('meta[name="request-verification-token"]')?.content || '';
    const notifications = new Map();

    const updateCount = function (serverCount) {
      const unreadCount = typeof serverCount === 'number'
        ? serverCount
        : Array.from(notifications.values()).filter(function (item) {
          return !item.isRead;
        }).length;

      count.textContent = unreadCount.toString();
      count.classList.toggle('d-none', unreadCount === 0);
    };

    const formatTime = function (value) {
      const date = new Date(value);
      if (Number.isNaN(date.getTime())) {
        return '';
      }

      return date.toLocaleString('uk-UA', {
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        month: '2-digit'
      });
    };

    const render = function () {
      list.innerHTML = '';

      const items = Array.from(notifications.values()).sort(function (a, b) {
        return new Date(b.createdAt) - new Date(a.createdAt);
      });

      if (items.length === 0) {
        const empty = document.createElement('div');
        empty.className = 'notification-empty';
        empty.textContent = 'Нових сповіщень немає.';
        list.appendChild(empty);
        updateCount();
        return;
      }

      for (const item of items.slice(0, 10)) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = `notification-item${item.isRead ? '' : ' is-unread'}`;
        button.dataset.id = item.id;

        const title = document.createElement('span');
        title.className = 'notification-item__title';
        title.textContent = item.title;

        const message = document.createElement('span');
        message.className = 'notification-item__message';
        message.textContent = item.message;

        const time = document.createElement('span');
        time.className = 'notification-item__time';
        time.textContent = formatTime(item.createdAt);

        button.append(title, message, time);
        list.appendChild(button);
      }

      updateCount();
    };

    const addNotification = function (item) {
      if (!item || typeof item.id !== 'number') {
        return;
      }

      notifications.set(item.id, item);
      render();
      updateCount();
    };

    const showToast = function (item) {
      let stack = document.querySelector('.notification-toast-stack');
      if (!stack) {
        stack = document.createElement('div');
        stack.className = 'notification-toast-stack';
        document.body.appendChild(stack);
      }

      const toast = document.createElement('div');
      toast.className = 'notification-toast';
      toast.setAttribute('role', 'status');

      const title = document.createElement('strong');
      title.textContent = item.title;

      const message = document.createElement('div');
      message.className = 'notification-item__message';
      message.textContent = item.message;

      toast.append(title, message);
      stack.prepend(toast);

      window.setTimeout(function () {
        toast.remove();
      }, 6500);
    };

    const loadNotifications = async function () {
      const response = await fetch('/notifications');
      if (!response.ok) {
        return;
      }

      const data = await response.json();
      notifications.clear();

      for (const item of data.notifications || []) {
        notifications.set(item.id, item);
      }

      render();
    };

    const markAsRead = async function (id) {
      const item = notifications.get(id);
      if (!item || item.isRead) {
        return;
      }

      item.isRead = true;
      render();

      await fetch(`/notifications/read/${id}`, {
        method: 'POST',
        headers: {
          RequestVerificationToken: token
        }
      });
    };

    list.addEventListener('click', function (event) {
      const item = event.target.closest('.notification-item');
      if (!item) {
        return;
      }

      markAsRead(Number(item.dataset.id));
    });

    loadNotifications();

    if (!window.signalR) {
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/notificationsHub')
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', function (item) {
      addNotification(item);
      showToast(item);
    });

    connection.start().catch(function () {
      // The dropdown still works from persisted notifications if realtime is unavailable.
    });
  });
})();
