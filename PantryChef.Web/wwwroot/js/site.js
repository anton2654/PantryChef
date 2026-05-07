(function () {
  document.addEventListener('click', function (event) {
    const trigger = event.target.closest('[data-confirm]');
    if (!trigger) {
      return;
    }

    const message = trigger.getAttribute('data-confirm');
    if (message && !window.confirm(message)) {
      event.preventDefault();
    }
  });
})();
