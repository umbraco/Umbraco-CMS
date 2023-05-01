import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * The base URL of the configured Umbraco server.
 * If the server is local, this will be an empty string.
 *
 * @remarks This is the base URL of the Umbraco server, not the base URL of the backoffice.
 *
 * @example https://localhost:44300
 * @example https://my-umbraco-site.com
 * @example ''
 */
export const UMB_SERVER_URL = new UmbContextToken<string>(
	'UmbServerUrl',
	'The base URL of the configured Umbraco server.'
);
