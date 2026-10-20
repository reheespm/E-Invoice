// Shows the site root in the address bar for every application screen, so screen paths and record ids are not visible.
// Routing is unchanged: pages still have their own routes and permission checks; only the displayed URL is replaced.
// Refresh / Back therefore return to the dashboard. Account pages are left alone because their forms post to the current URL.
(function () {
    const root = new URL(document.baseURI).pathname;
    const unmasked = /^\/account(\/|$)/i;

    function mask() {
        const path = location.pathname;
        if (path !== root && !unmasked.test(path.substring(root.length - 1))) {
            history.replaceState(history.state, '', root);
        }
    }

    mask();
    Blazor.addEventListener('enhancedload', mask);
})();
