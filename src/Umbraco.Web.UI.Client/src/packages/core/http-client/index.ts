import { client } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Pre-configure the client with default credentials for cookie-based authentication.
 * This ensures all requests include cookies by default, which is required for
 * cookie-based authentication in Umbraco 17.0+.
 *
 * Extensions using this client will automatically get credentials: 'include'.
 */
client.setConfig({
	credentials: 'include',
});

export { client as umbHttpClient };
