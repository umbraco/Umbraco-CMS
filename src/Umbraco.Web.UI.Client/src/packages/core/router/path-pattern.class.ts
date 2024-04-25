import { umbUrlPatternToString } from '../utils/path/url-pattern-to-string.function.js';

export type UmbPathPatternParamsType = { [key: string]: any };

export class UmbPathPattern<LocalParamsType extends UmbPathPatternParamsType = UmbPathPatternParamsType> {
	#local: string;
	#base: string;

	/**
	 * Get the params type of the path pattern
	 *
	 * @public
	 * @type      {T}
	 * @memberOf  UmbPathPattern
	 * @example   `typeof MyPathPattern.PARAMS`
	 * @returns   undefined
	 */
	readonly PARAMS!: LocalParamsType;

	constructor(localPattern: string, basePath?: string) {
		this.#local = localPattern;
		this.#base = basePath ? (basePath.lastIndexOf('/') !== basePath.length - 1 ? basePath + '/' : basePath) : '';
	}

	generateLocal(params: LocalParamsType) {
		return umbUrlPatternToString(this.#local, params);
	}
	generateGlobal(params: LocalParamsType) {
		return this.#base + umbUrlPatternToString(this.#local, params);
	}

	toString() {
		return this.#local;
	}
}
