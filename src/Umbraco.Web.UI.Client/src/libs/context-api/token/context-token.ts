export class UmbContextToken<T = unknown> {
	/**
	 * Get the type of the token
	 *
	 * @public
	 * @type      {T}
	 * @memberOf  UmbContextToken
	 * @example   `typeof MyToken.TYPE`
	 * @returns   undefined
	 */
	readonly TYPE: T = undefined as never;

	/**
	 * @param alias   Unique identifier for the token,
	 * @param _desc   Description for the token,
	 *                used only for debugging purposes,
	 *                it should but does not need to be unique
	 */
	constructor(protected alias: string, protected _desc?: string) {}

	/**
	 * This method must always return the unique alias of the token since that
	 * will be used to look up the token in the injector.
	 *
	 * @returns the unique alias of the token
	 */
	toString(): string {
		return this.alias;
	}
}
