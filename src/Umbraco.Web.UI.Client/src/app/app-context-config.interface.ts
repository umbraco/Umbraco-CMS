/**
 * Configuration interface for the Umbraco App Context
 * @export
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
}
