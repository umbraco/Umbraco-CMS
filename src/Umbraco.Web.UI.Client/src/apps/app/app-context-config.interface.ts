import type { UmbServerConnection } from '../../packages/core/server/server-connection.js';

/**
 * Configuration interface for the Umbraco App Context
 * @interface UmbAppContextConfig
 */
export interface UmbAppContextConfig {
	/**
	 * The base URL of the configured Umbraco server.
	 * @type {string}
	 * @memberof UmbAppContextConfig
	 */
	serverUrl: string;

	/**
	 * The base path of the backoffice.
	 * @type {string}
	 * @memberof UmbAppContextConfig
	 */
	backofficePath: string;

	/**
	 * Configuration for the server connection.
	 * @memberof UmbAppContextConfig
	 */
	serverConnection: UmbServerConnection;
}
