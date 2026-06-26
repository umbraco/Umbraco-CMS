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
}
