export class ContextToken<T = unknown> {
	/**
	 * @param _desc   Description for the token,
	 *                used only for debugging purposes,
	 *                it should but does not need to be unique
	 */
	constructor(protected _desc: string) {}

	/**
	 * @internal
	 */
	get multi(): ContextToken<Array<T>> {
		return this as ContextToken<Array<T>>;
	}

	toString(): string {
		return `${ContextToken.name} ${this._desc}`;
	}
}
