window.adminTheme = {
    get: function () {
        return localStorage.getItem('admin-dark-mode') === 'true';
    },
    set: function (isDark) {
        localStorage.setItem('admin-dark-mode', isDark ? 'true' : 'false');
    }
};
