// Links to these resource types cannot be followed inside the sandboxed preview iframe
// (Chrome blocks downloads and inline PDF rendering from sandboxed contexts), so the
// preview shell intercepts them and opens them in a new top-level tab instead.
const NON_HTML_RESOURCE_PATTERN =
	/\.(pdf|doc|docx|xls|xlsx|ppt|pptx|csv|txt|rtf|zip|7z|rar|tar|gz|mp3|wav|ogg|opus|flac|aiff|mp4|mov|webm|ogv|mkv|avi|wmv)$/i;

// Targets the preview sandbox cannot satisfy: it omits both `allow-popups` (so
// `_blank` silently fails) and `allow-top-navigation` (so `_top`/`_parent` are
// blocked). Anchors aiming at any of these need to be re-issued via the parent.
const SANDBOX_BLOCKED_TARGETS = new Set(['_blank', '_top', '_parent']);

// Tracks documents that already have a click interceptor so that repeated `load` events
// for the same Document (e.g. back/forward cache restoration) don't stack listeners and
// trigger multiple window.open calls per click. WeakSet entries are GC'd with the doc.
const instrumentedDocuments = new WeakSet<Document>();

/**
 * Attaches a click listener to the iframe document that intercepts links the sandbox
 * would block — non-HTML downloads (PDFs, archives, media), explicit `download` links,
 * cross-origin navigations, and anchors targeting `_blank`/`_top`/`_parent` — and opens
 * them in a new top-level tab via the un-sandboxed parent shell. Same-origin HTML links
 * without a blocked target flow through unchanged so the previewed site remains
 * navigable inside the iframe. Idempotent per Document.
 *
 * Runs in the capture phase and intercepts unconditionally for matched links, so SPA
 * routers that preventDefault on all anchor clicks (for client-side navigation) do not
 * silence this handler.
 * @param {HTMLIFrameElement} iframe The iframe element.
 */
export function attachLinkInterceptor(iframe: HTMLIFrameElement): void {
	const doc = iframe.contentDocument;
	if (!doc) return;
	if (instrumentedDocuments.has(doc)) return;
	instrumentedDocuments.add(doc);

	doc.addEventListener(
		'click',
		(event) => {
			const link = (event.target as Element | null)?.closest('a');
			if (!link?.href) return;

			const url = new URL(link.href);
			// Non-http(s) schemes (mailto:, tel:, etc.) are handed off to the OS and
			// must not be re-opened via window.open — that would spawn a blank tab.
			const isWebScheme = url.protocol === 'http:' || url.protocol === 'https:';

			// Compare against the document's base URL rather than location.origin so
			// the check stays correct when the iframe is about:blank/srcdoc and inherits
			// its base URL from the parent (e.g. in unit tests).
			const docOrigin = new URL(doc.baseURI).origin;

			const isDownload = link.hasAttribute('download');
			const isNonHtmlResource = NON_HTML_RESOURCE_PATTERN.test(url.pathname);
			const isCrossOrigin = isWebScheme && url.origin !== docOrigin;
			const hasBlockedTarget = isWebScheme && SANDBOX_BLOCKED_TARGETS.has(link.target);

			if (!isDownload && !isNonHtmlResource && !isCrossOrigin && !hasBlockedTarget) return;

			event.preventDefault();
			event.stopImmediatePropagation();
			window.open(link.href, '_blank', 'noopener,noreferrer');
		},
		{ capture: true },
	);
}
