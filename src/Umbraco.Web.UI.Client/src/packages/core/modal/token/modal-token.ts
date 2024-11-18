import type { UmbModalConfig } from '../types.js';

export interface UmbModalTokenDefaults<
	ModalDataType extends { [key: string]: any } = { [key: string]: any },
	ModalValueType = unknown,
> {
	modal?: UmbModalConfig;
	data?: ModalDataType;
	value?: ModalValueType;
}

export class UmbModalToken<
	ModalDataType extends { [key: string]: any } = { [key: string]: any },
	ModalValueType = unknown,
> {
	/**
	 * Get the data type of the token's data.
	 * @public
	 * @type      {ModalDataType}
	 * @memberOf  UmbModalToken
	 * @example   `typeof MyModal.TYPE`
	 * @returns   undefined
	 */
	readonly DATA: ModalDataType = undefined as never;

	/**
	 * Get the value type of the token
	 * @public
	 * @type      {ModalValueType}
	 * @memberOf  UmbModalToken
	 * @example   `typeof MyModal.VALUE`
	 * @returns   undefined
	 */
	readonly VALUE: ModalValueType = undefined as never;

	#alias;
	#defaults;

	/**
	 * @param alias   Unique identifier for the token,
	 * @param defaults  Defaults for the modal,
	 */
	constructor(alias: string, defaults?: UmbModalTokenDefaults<ModalDataType, ModalValueType>) {
		this.#alias = alias;
		this.#defaults = defaults;
	}

	/**
	 * This method must always return the unique alias of the token since that
	 * will be used to look up the token in the injector.
	 * @returns the unique alias of the token
	 */
	toString(): string {
		return this.#alias;
	}

	public getDefaultModal(): UmbModalConfig | undefined {
		return this.#defaults?.modal;
	}

	public getDefaultData(): ModalDataType | undefined {
		return this.#defaults?.data;
	}

	public getDefaultValue(): ModalValueType | undefined {
		return this.#defaults?.value;
	}
}
