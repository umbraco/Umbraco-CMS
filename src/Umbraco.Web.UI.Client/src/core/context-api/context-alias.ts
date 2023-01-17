export class UmbContextAlias<T = unknown> {
	/**
	 * @param _desc   Description for the token,
	 *                used only for debugging purposes,
	 *                it should but does not need to be unique
	 */
	constructor(protected _desc: string) {}

	/**
	 * @internal
	 */
	get multi(): UmbContextAlias<Array<T>> {
		return this as UmbContextAlias<Array<T>>;
	}

	toString(): string {
		return `${UmbContextAlias.name} ${this._desc}`;
	}
}
