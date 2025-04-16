/**
 * Check if the current window has an opener window with the same origin and optional pathname.
 * This is useful for checking if the current window was opened by another window from within the same application.
 * @remark If the current window was opened by another window, the opener window is accessible via `window.opener`.
 * @remark There could still be an opener if the opener window is closed or navigated away or if the opener window is not from the same origin,
 * but this function will only return `true` if the opener window is accessible and has the same origin and optional pathname.
 * @param pathname Optional pathname to check if the opener window has the same pathname.
 * @param windowLike The window-like object to use for checking the opener. Default is `window`.
 * @returns `true` if the current window has an opener window with the same origin and optional pathname, otherwise `false`.
 */
export function hasOwnOpener(pathname?: string, windowLike: Window = globalThis.window): boolean {
	try {
		const opener = windowLike.opener;
		if (!opener) {
			return false;
		}

		const openerLocation = opener.location;
		const currentLocation = windowLike.location;

		if (openerLocation.origin !== currentLocation.origin) {
			return false;
		}

		// If there is a pathname, check if the opener has the same pathname
		if (typeof pathname !== 'undefined' && !openerLocation.pathname.startsWith(pathname)) {
			return false;
		}

		return true;
	} catch {
		// If there is a security error, it means that the opener is from a different origin, so we let it fall through
		return false;
	}
}
