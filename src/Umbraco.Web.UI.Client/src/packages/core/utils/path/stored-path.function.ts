import { ensureLocalPath } from './ensure-local-path.function.js';

export const UMB_STORAGE_REDIRECT_URL = 'umb:auth:redirect';

/**
 * Retrieve the stored path from the session storage.
 * @remark This is used to redirect the user to the correct page after login.
 */
export function retrieveStoredPath(): URL | null {
	let currentRoute = '';
	const savedRoute = sessionStorage.getItem(UMB_STORAGE_REDIRECT_URL);
	if (savedRoute) {
		sessionStorage.removeItem(UMB_STORAGE_REDIRECT_URL);
		currentRoute = savedRoute.endsWith('logout') ? currentRoute : savedRoute;
	}

	return currentRoute ? ensureLocalPath(currentRoute) : null;
}

/**
 * Store the path in the session storage.
 * @param path
 * @remark This is used to redirect the user to the correct page after login.
 * @remark The path must be a local path, otherwise it is not stored.
 */
export function setStoredPath(path: string): void {
	const url = new URL(path, window.location.origin);
	if (url.origin !== window.location.origin) {
		return;
	}
	sessionStorage.setItem(UMB_STORAGE_REDIRECT_URL, url.toString());
}

/**
 * Redirect the user to the stored path or the base path if not available.
 * If the basePath matches the start of the stored path, the browser will replace the state instead of redirecting.
 * @param {string} basePath - The base path to redirect to if no stored path is available.
 * @param {boolean} force - If true, will redirect using Location
 */
export function redirectToStoredPath(basePath: string, force = false): void {
	const url = retrieveStoredPath();
	const isBackofficePath = url?.pathname.startsWith(basePath) ?? false;

	if (isBackofficePath && !force) {
		history.replaceState(null, '', url?.toString() ?? '');
	} else {
		window.location.href = url?.toString() ?? basePath;
	}
}
