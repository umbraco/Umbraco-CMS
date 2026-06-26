import type { UmbServerConnection } from './server-connection.js';

export interface UmbServerContextConfig {
	/**
	 * The server URL to use for the context.
	 */
	serverUrl: string;
	/**
	 * The backoffice path to use for the context.
	 */
	backofficePath: string;
	/**
	 * The server connection to use for the context.
	 */
	serverConnection: UmbServerConnection;
	/**
	 * An optional host-controlled cache-buster appended (as `umb__rnd`) to package `/App_Plugins` assets.
	 */
	cacheBuster?: string;
}
