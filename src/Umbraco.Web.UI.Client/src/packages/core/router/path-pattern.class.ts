import { umbUrlPatternToString } from '../utils/path/url-pattern-to-string.function.js';

export type UmbPathPatternParamsType = { [key: string]: any };

// Replaces property union type null with 'null' [NL]
type ReplaceNull<T> = T extends null ? 'null' : T;
export type UmbPathPatternTypeAsEncodedParamsType<T> = {
	[P in keyof T]: T[P] extends (infer R)[] ? ReplaceNull<R>[] : ReplaceNull<T[P]>;
};

export class UmbPathPattern<
	LocalParamsType extends UmbPathPatternParamsType = UmbPathPatternParamsType,
	BaseParamsType extends UmbPathPatternParamsType = LocalParamsType,
> {
	#local: string;
	#base: string;

	/**
	 * Get the params type of the path pattern
	 * @public
	 * @type      {T}
	 * @memberOf  UmbPathPattern
	 * @example   `typeof MyPathPattern.PARAMS`
	 */
	readonly PARAMS!: LocalParamsType;

	/**
	 * Get absolute params type of the path pattern
	 * @public
	 * @type      {T}
	 * @memberOf  UmbPathPattern
	 * @example   `typeof MyPathPattern.ABSOLUTE_PARAMS`
	 */
	readonly ABSOLUTE_PARAMS!: LocalParamsType & BaseParamsType;

	constructor(localPattern: string, basePath?: UmbPathPattern | string) {
		this.#local = localPattern;
		basePath = basePath?.toString() ?? '';
		this.#base = basePath.lastIndexOf('/') !== basePath.length - 1 ? basePath + '/' : basePath;
	}

	generateLocal(params: LocalParamsType) {
		return umbUrlPatternToString(this.#local, params);
	}
	/**
	 * generate an absolute path from the path pattern
	 * @param params
	 * @param baseParams
	 * @returns
	 */
	generateAbsolute(params: LocalParamsType & BaseParamsType) {
		return (
			(this.#base.indexOf(':') !== -1 ? umbUrlPatternToString(this.#base, params) : this.#base) +
			umbUrlPatternToString(this.#local, params)
		);
	}

	toString() {
		return this.#local;
	}
}
