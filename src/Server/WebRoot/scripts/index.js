if (typeof trustedTypes !== "undefined") {
    trustedTypes.createPolicy('default', {
        createHTML: (string, sink) => DOMPurify.sanitize(string, { RETURN_TRUSTED_TYPE: true })
    });
    const policy = trustedTypes.createPolicy('myPolicy', { createScriptURL: (s) => s });
    const url = policy.createScriptURL('/scripts/sw.js');
    if ("serviceWorker" in navigator) {
        navigator.serviceWorker.register(url);
    }
}
else {
    if ("serviceWorker" in navigator) {
        navigator.serviceWorker.register('/scripts/sw.js');
    }
}

function attachShadowRoots(root) {
    root.querySelectorAll("template[shadowrootmode]").forEach(template => {
        let shadowRoot = root.shadowRoot
        if (root.shadowRoot === null) {
            const mode = template.getAttribute("shadowrootmode");
            shadowRoot = template.parentNode.attachShadow({ mode });
        }
        shadowRoot.appendChild(template.content);
        template.remove();
        attachShadowRoots(shadowRoot);
    })
}
attachShadowRoots(document.body);