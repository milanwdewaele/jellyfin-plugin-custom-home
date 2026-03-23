(function () {
    const pluginId = "c3cf7f0e-5bb0-4de4-9689-4c8f6e2fd111";

    async function loadConfig() {
        if (!window.ApiClient) {
            return null;
        }

        try {
            const url = window.ApiClient.getUrl("Plugins/" + pluginId + "/Configuration");
            return await window.ApiClient.getJSON(url);
        } catch (e) {
            console.error("CustomHome hero: failed to load configuration", e);
            return null;
        }
    }

    function getCurrentUserName() {
        try {
            if (window.ApiClient && window.ApiClient.getCurrentUser) {
                const user = window.ApiClient.getCurrentUser();
                return (user && (user.Name || user.Username)) || "";
            }
        } catch (e) {
            console.warn("CustomHome hero: failed to resolve username", e);
        }
        return "";
    }

    function buildGreetingTitle(config) {
        const template = config && config.GreetingHeroTitleTemplate
            ? config.GreetingHeroTitleTemplate
            : "Goeiedag, {UserName}";
        const name = getCurrentUserName();
        return template.replace("{UserName}", name || "").trim();
    }

    function pickSubtitle(config) {
        const options = (config && config.GreetingHeroSubtitleOptions) || [];
        if (!options.length) {
            return "";
        }
        const idx = Math.floor(Math.random() * options.length);
        return options[idx];
    }

    function createHeroElement(title, subtitle) {
        const section = document.createElement("section");
        section.id = "custom-home-hero";
        section.className = "homeSection custom-home-hero";

        const inner = document.createElement("div");
        inner.className = "custom-home-hero-inner";

        const titleEl = document.createElement("div");
        titleEl.className = "custom-home-hero-title";
        titleEl.textContent = title;

        const subtitleEl = document.createElement("div");
        subtitleEl.className = "custom-home-hero-subtitle";
        subtitleEl.textContent = subtitle;

        inner.appendChild(titleEl);
        if (subtitle && subtitle.length) {
            inner.appendChild(subtitleEl);
        }

        section.appendChild(inner);
        return section;
    }

    async function tryRenderHero() {
        if (!window.ApiClient) {
            return;
        }

        if (document.getElementById("custom-home-hero")) {
            return;
        }

        const homeSections = document.querySelector(".homeSectionsContainer");
        if (!homeSections) {
            return;
        }

        const config = await loadConfig();
        if (!config || config.EnableHeroSection === false) {
            return;
        }

        const title = buildGreetingTitle(config);
        const subtitle = pickSubtitle(config);

        const hero = createHeroElement(title, subtitle);

        if (homeSections.firstChild) {
            homeSections.insertBefore(hero, homeSections.firstChild);
        } else {
            homeSections.appendChild(hero);
        }
    }

    function scheduleInit() {
        if (!window.location || !window.location.hash) {
            return;
        }

        const hash = window.location.hash.toLowerCase();
        const isHome = hash === "#/home" || hash === "#/home.html";
        if (!isHome) {
            return;
        }

        let attempts = 0;
        const maxAttempts = 40; // ~20s bij 500ms interval

        const intervalId = setInterval(() => {
            attempts++;
            tryRenderHero().then((rendered) => { /* no-op */ });

            if (document.getElementById("custom-home-hero") || attempts >= maxAttempts) {
                clearInterval(intervalId);
            }
        }, 500);
    }

    document.addEventListener("DOMContentLoaded", scheduleInit);
    window.addEventListener("hashchange", scheduleInit);
})();
