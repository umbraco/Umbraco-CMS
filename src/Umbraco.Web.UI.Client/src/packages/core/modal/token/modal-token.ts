import { UmbModalConfig } from '../modal-manager.context.js';

export class UmbModalToken<ModalDataType extends object = object, ModalValueType = unknown> {
	/**
	 * Get the data type of the token's data.
	 *
	 * @public
	 * @type      {ModalDataType}
	 * @memberOf  UmbModalToken
	 * @example   `typeof MyModal.TYPE`
	 * @returns   undefined
	 */
	readonly DATA: ModalDataType = undefined as never;

	/**
	 * Get the value type of the token
	 *
	 * @public
	 * @type      {ModalValueType}
	 * @memberOf  UmbModalToken
	 * @example   `typeof MyModal.VALUE`
	 * @returns   undefined
	 */
	readonly VALUE: ModalValueType = undefined as never;

	/**
	 * @param alias   Unique identifier for the token,
	 * @param defaultConfig  Default configuration for the modal,
	 * @param defaultData  Default data for the modal,
	 */
	constructor(
		protected alias: string,
		protected defaultConfig?: UmbModalConfig,
		protected defaultData?: ModalDataType,
		protected defaultValue?: ModalValueType,
	) {}

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

	public getDefaultData(): ModalDataType | undefined {
		return this.defaultData;
	}

	public getDefaultValue(): ModalValueType | undefined {
		return this.defaultValue;
	}
}
