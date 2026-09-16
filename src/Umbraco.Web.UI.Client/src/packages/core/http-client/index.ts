import { client } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Pre-configure the shared backend API client for the cookie-based backoffice (Umbraco 17+).
 *
 * - `credentials: 'include'` sends the httpOnly authentication cookie with every request — the cookie
 *   is the sole credential, so no `Authorization` header is needed.
 * - `redirect: 'manual'` stops the client from following redirects. The Management API is JSON-only,
 *   so any 3xx is an auth-challenge bounce to the HTML login page; following it would return login
 *   HTML that breaks JSON parsing in early loaders (e.g. the extension manifests). Manual mode turns
 *   a stray 3xx into an opaque, non-ok response instead. Defence in depth — the server also answers
 *   API requests with 401/403 directly rather than redirecting.
 *
 * Configured at module level so it applies regardless of UmbAuthContext initialisation timing.
 */
client.setConfig({
	credentials: 'include',
	redirect: 'manual',
});

/**
 * The pre-configured client used by generated SDK functions and for custom backend API calls.
 * Authentication rides on the httpOnly session cookie (`credentials: 'include'`) — there is no bearer
 * token. Configured for the backoffice context only; other contexts (e.g. a public website) need
 * their own client configuration.
 */
export { client as umbHttpClient };

/**
 * Structural type representing any `@hey-api/openapi-ts` generated client.
 *
 * Each call to the generator produces a fully-bound `Client<RequestFn, Config, …>`
 * tied to that document's specific operation/options shapes, so the backoffice's
 * own `umbHttpClient` and the client in an extension package's `Client/src/api/`
 * are structurally identical but not assignable to each other under TypeScript's
 * variance rules. Use `UmbApiClient` on APIs (such as `UmbAuthContext.configureClient`)
 * that need to accept either.
 *
 * The shape only covers what those APIs touch — `setConfig`, `request`, and the
 * three interceptor middlewares — so callers retain meaningful autocomplete on the
 * concrete client they pass in, while we keep the public surface flexible.
 */
/* eslint-disable @typescript-eslint/no-explicit-any -- bivariant escape hatch: see jsdoc above */
export type UmbApiClient = {
	setConfig: (config: any) => any;
	request: (options: any) => any;
	interceptors: {
		request: { use: (fn: any) => unknown; eject: (fn: any) => unknown };
		response: { use: (fn: any) => unknown; eject: (fn: any) => unknown };
		error: { use: (fn: any) => unknown; eject: (fn: any) => unknown };
	};
};
/* eslint-enable @typescript-eslint/no-explicit-any */
