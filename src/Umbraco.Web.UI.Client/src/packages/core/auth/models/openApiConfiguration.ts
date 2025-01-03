/**
 * Configuration for the OpenAPI (Umbraco) server. This is used to communicate with the Management API.
 * This is useful if you want to configure your Fetch, Axios or other HTTP client to communicate with the Management API.
 * If you use the recommended resource generator [openapi-typescript-codegen](https://github.com/ferdikoomen/openapi-typescript-codegen) this can be used to configure the `OpenAPI` object.
 */
export interface UmbOpenApiConfiguration {
	/**
	 * The base URL of the OpenAPI (Umbraco) server.
	 */
	readonly base: string;

	/**
	 * The configured version of the Management API to use.
	 */
	readonly version: string;

	/**
	 * The `withCredentials` option for the Fetch API.
	 */
	readonly withCredentials: boolean;

	/**
	 * The `credentials` option for the Fetch API.
	 */
	readonly credentials: 'include' | 'omit' | 'same-origin';

	/**
	 * The token to use for the Authorization header.
	 * @returns A resolver for the token to use for the Authorization header.
	 */
	readonly token: () => Promise<string>;

	/**
	 * The encoder to use for the request URLs.
	 * @returns A resolver for the encoder to use for the request URLs.
	 */
	readonly encodePath?: (value: string) => string;
}
