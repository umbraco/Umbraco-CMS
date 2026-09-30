import type { CreateClientConfig } from './api/client.gen';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

/**
 * Pre-configure the generated client by copying the Umbraco HTTP client's config.
 *
 * Called automatically by the generated client during initialization (via runtimeConfigPath).
 * Inherits baseUrl, credentials, auth, and headers from umbHttpClient — which is already
 * configured by the backoffice before extensions load.
 *
 * @see https://heyapi.dev/openapi-ts/clients/fetch#configuration
 */
export const createClientConfig: CreateClientConfig = (config) => ({
	...config,
	...umbHttpClient.getConfig(),
});
