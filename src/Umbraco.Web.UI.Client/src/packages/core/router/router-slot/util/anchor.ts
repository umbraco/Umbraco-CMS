/**
 * Hook up a click listener to the window that, for all anchor tags (HTML or SVG)
 * that has a relative HREF, uses the history API instead.
 */
export function ensureAnchorHistory() {
	const isMac = /Mac|iPhone|iPad|iPod/.test(navigator.userAgent);

	window.addEventListener('click', (e: MouseEvent) => {
		// If we try to open link in a new tab, we want to skip:
		// - Mac uses Meta (⌘) + Click
		// - Windows and Linux use Ctrl + Click
		if ((isMac && e.metaKey) || (!isMac && e.ctrlKey)) return;

		// Find the target by using the composed path to get the element through the shadow boundaries.
		// Support both HTML anchor tags and SVG anchor tags
		const $anchor = (('composedPath' in e) as any)
			? e.composedPath().find(($elem) => $elem instanceof HTMLAnchorElement || $elem instanceof SVGAElement)
			: e.target;

		// Abort if the event is not about an anchor tag (HTML or SVG)
		if ($anchor == null || !($anchor instanceof HTMLAnchorElement || $anchor instanceof SVGAElement)) {
			return;
		}

		// Get the HREF value from the anchor tag
		// SVGAElement.href returns SVGAnimatedString, so we need to access .baseVal
		const href = $anchor instanceof SVGAElement ? $anchor.href.baseVal : $anchor.href;
		const target = $anchor instanceof SVGAElement ? $anchor.target.baseVal : $anchor.target;

		// For SVG anchors, we need to construct a full URL to extract pathname, search, and hash
		// For HTML anchors, these properties are directly available
		let fullUrl: URL;
		try {
			// Use the current document base URI as the base to resolve relative URLs
			// This respects the <base> tag and works the same as HTML anchors
			// Note: This may resolve into an external URL, but we validate that later
			fullUrl = new URL(href, document.baseURI);
		} catch {
			// Invalid URL, skip
			return;
		}

		// Only handle the anchor tag if the follow holds true:
		// - The HREF is relative to the origin of the current location.
		// - The target is targeting the current frame.
		// - The anchor doesn't have the attribute [data-router-slot]="disabled"
		if (
			fullUrl.origin !== location.origin ||
			(target !== '' && target !== '_self') ||
			$anchor.dataset['routerSlot'] === 'disabled'
		) {
			return;
		}

		// Prevent the default behavior
		e.preventDefault();

		// Change the history!
		history.pushState(null, '', fullUrl);
	});
}
