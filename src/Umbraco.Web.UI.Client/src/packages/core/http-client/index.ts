import { client } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Pre-configure the client for cookie-based authentication (Umbraco 17.0+).
 *
 * - `credentials: 'include'` ensures cookies are sent with every request.
 * - `auth` returns `'[redacted]'` which triggers `HideBackOfficeTokensHandler` on the
 *    server to swap the real token from the httpOnly cookie into the request pipeline.
 *    This is used by generated SDK functions that include `security` metadata.
 *
 * The cookie carries the actual token; the Authorization header tells the server to
 * look for it. Setting these at module level eliminates any timing dependency on
 * UmbAuthContext initialization.
 */
client.setConfig({
	credentials: 'include',
	auth: () => '[redacted]',
});

/**
 * Export the pre-configured client for use in the backoffice. This client will be used by generated SDK functions that include `security` metadata, and can also be used directly for custom API calls.
 * Note: This client is configured for cookie-based authentication and should only be used in the backoffice context where such authentication is applicable. For other contexts (e.g., public website), a different client configuration may be necessary.
 * Note: To use the 'auth' option to send a Bearer token, the call must specify a security scheme in the OpenAPI spec, and the generated SDK function must include `security` metadata, or you must specify a security scheme manually. Otherwise, the 'auth' option will not be applied to the request.
 * @example
 * ```js
 * umbHttpClient.get({
 *   url: '/some/protected/endpoint',
 *   security: [{ type: 'http', scheme: 'bearer' }] // This tells the client to apply the 'auth' option
 * }).then(response => {
 *   // handle response
 * });
 * ```
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
