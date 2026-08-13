/**
 * Client routes worth returning to after a login: the back office proper (`section/:sectionName` —
 * see `UMB_SECTION_PATH_PATTERN`) and the other routes behind the auth guard. Everything else is a
 * boot route (install, logout, error, auth-callback) that either renders without a session — so
 * returning there would just show the login screen again — or isn't a destination at all.
 *
 * Deliberately an allowlist: omitting a returnable route only costs a deep link (the user lands on
 * the back office root), while omitting a session-less one loops the login.
 */
const RETURNABLE_ROUTES = new Set(['section', 'upgrade', 'preview']);

/**
 * Whether a location is somewhere the user should be returned to after logging in.
 * @param {string} pathname The location to test, e.g. `window.location.pathname`.
 * @param {string} backofficePath The configured back-office path, so this holds whether the client
 * is served at "/" or "/umbraco".
 * @returns {boolean} True for the back office's section routes and the other guarded routes.
 */
export function isReturnableRoute(pathname: string, backofficePath: string): boolean {
	const route = pathname.startsWith(backofficePath) ? pathname.slice(backofficePath.length) : pathname;

	const firstSegment = route.split('/').find(Boolean);

	return firstSegment !== undefined && RETURNABLE_ROUTES.has(firstSegment);
}
