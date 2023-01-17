export class ContextAlias<T = unknown> {
	/**
	 * @param _desc   Description for the token,
	 *                used only for debugging purposes,
	 *                it should but does not need to be unique
	 */
	constructor(protected _desc: string) {}

	/**
	 * @internal
	 */
	get multi(): ContextAlias<Array<T>> {
		return this as ContextAlias<Array<T>>;
	}

	toString(): string {
		return `${ContextAlias.name} ${this._desc}`;
	}
}
