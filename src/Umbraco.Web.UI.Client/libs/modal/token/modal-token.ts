import { UmbModalConfig } from '../modal.context';

export class UmbModalToken<Data extends object = object, Result = unknown> {
	/**
	 * Get the data type of the token's data.
	 *
	 * @public
	 * @type      {Data}
	 * @memberOf  UmbModalToken
	 * @example   `typeof MyModal.TYPE`
	 * @returns   undefined
	 */
	readonly DATA: Data = undefined as never;

	/**
	 * Get the result type of the token
	 *
	 * @public
	 * @type      {Result}
	 * @memberOf  UmbModalToken
	 * @example   `typeof MyModal.RESULT`
	 * @returns   undefined
	 */
	readonly RESULT: Result = undefined as never;

	/**
	 * @param alias   Unique identifier for the token,
	 * @param defaultConfig  Default configuration for the modal,
	 * @param _desc   Description for the token,
	 *                used only for debugging purposes,
	 *                it should but does not need to be unique
	 */
	constructor(protected alias: string, protected defaultConfig?: UmbModalConfig, protected _desc?: string) {}

	/**
	 * This method must always return the unique alias of the token since that
	 * will be used to look up the token in the injector.
	 *
	 * @returns the unique alias of the token
	 */
	toString(): string {
		return this.alias;
	}

	public getDefaultConfig(): UmbModalConfig | undefined {
		return this.defaultConfig;
	}
}
