/**
 * Hook up a click listener to the window that, for all anchor tags
 * that has a relative HREF, uses the history API instead.
 */
export function ensureAnchorHistory() {
	const isWindows = navigator.platform.toUpperCase().indexOf('WIN') !== -1;

	window.addEventListener('click', (e: MouseEvent) => {
		// If we try to open link in a new tab, then we want to skip skip:
		if ((isWindows && e.ctrlKey) || (!isWindows && e.metaKey)) return;

		// Find the target by using the composed path to get the element through the shadow boundaries.
		const $anchor = (('composedPath' in e) as any)
			? e.composedPath().find(($elem) => $elem instanceof HTMLAnchorElement)
			: e.target;

		// Abort if the event is not about the anchor tag
		if ($anchor == null || !($anchor instanceof HTMLAnchorElement)) {
			return;
		}

		// Get the HREF value from the anchor tag
		const href = $anchor.href;

		// Only handle the anchor tag if the follow holds true:
		// - The HREF is relative to the origin of the current location.
		// - The target is targeting the current frame.
		// - The anchor doesn't have the attribute [data-router-slot]="disabled"
		if (
			!href.startsWith(location.origin) ||
			($anchor.target !== '' && $anchor.target !== '_self') ||
			$anchor.dataset['routerSlot'] === 'disabled'
		) {
			return;
		}

		// Remove the origin from the start of the HREF to get the path
		const path = $anchor.pathname + $anchor.search + $anchor.hash;

		// Prevent the default behavior
		e.preventDefault();

		// Change the history!
		history.pushState(null, '', path);
	});
}
