// Shows the site root in the address bar for every application screen, so screen paths and record ids are never visible.
// Blazor fetches each screen from the URL it is given (not from the address bar), so only the URL written to history is
// replaced. Pages keep their own routes and permission checks. Refresh / Back therefore return to the dashboard.
// Account pages are left alone because their forms post to the current URL.
(function () {
    const root = new URL(document.baseURI).pathname;
    const unmasked = /^\/account(\/|$)/i;

    function maskedUrl(url) {
        if (url === undefined || url === null) {
            return url;
        }

        const target = new URL(url, location.href);
        if (target.origin !== location.origin || unmasked.test(target.pathname.substring(root.length - 1))) {
            return url;
        }

        return root;
    }

    for (const method of ['pushState', 'replaceState']) {
        const original = history[method].bind(history);
        history[method] = (state, title, url) => original(state, title, maskedUrl(url));
    }

    // Full page loads (typed address, login redirect) arrive on the real path.
    if (location.pathname !== maskedUrl(location.href)) {
        history.replaceState(history.state, '', location.href);
    }
})();
