import { client } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Pre-configure the client for cookie-based authentication (Umbraco 17.0+).
 *
 * - `credentials: 'include'` ensures cookies are sent with every request.
 * - `auth` returns `'[redacted]'` which triggers `HideBackOfficeTokensHandler` on the
 *    server to swap the real token from the httpOnly cookie into the request pipeline.
 *    This is used by generated SDK functions that include `security` metadata.
 * - `headers` sets a default `Authorization` header so that direct `.get()` / `.post()`
 *    calls (which lack `security` metadata) also include the token trigger.
 *
 * The cookie carries the actual token; the Authorization header tells the server to
 * look for it. Setting these at module level eliminates any timing dependency on
 * UmbAuthContext initialization.
 */
client.setConfig({
	credentials: 'include',
	auth: () => '[redacted]',
	headers: {
		Authorization: 'Bearer [redacted]',
	},
});

export { client as umbHttpClient };
