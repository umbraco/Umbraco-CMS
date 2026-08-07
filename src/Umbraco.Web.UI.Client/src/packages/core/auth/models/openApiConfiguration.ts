/**
 * Configuration for the OpenAPI (Umbraco) server. This is used to communicate with the Management API.
 * This is useful if you want to configure your Fetch, Axios or other HTTP client to communicate with the Management API.
 * If you use the recommended resource generator [openapi-typescript-codegen](https://github.com/ferdikoomen/openapi-typescript-codegen) this can be used to configure the `OpenAPI` object.
 */
export interface UmbOpenApiConfiguration {
	/**
	 * The base URL of the OpenAPI (Umbraco) server.
	 */
	readonly base?: string;

	/**
	 * The `credentials` option for the Fetch API.
	 */
	readonly credentials?: 'include' | 'omit' | 'same-origin';

	/**
	 * The token to use for the Authorization header.
	 * @deprecated Use `credentials: 'include'` instead, as the Management API uses cookie-based authentication. Scheduled for removal in Umbraco 21.
	 * @remarks Kept required until its removal, and resolves to undefined: the hey-api SDK omits the
	 * Authorization header rather than sending a value, so nothing is misled into thinking a token
	 * exists. The token accessors on UmbAuthContext were removed outright for that same reason.
	 * @returns A resolver for the token to use for the Authorization header.
	 */
	readonly token: () => Promise<string | undefined>;
}
