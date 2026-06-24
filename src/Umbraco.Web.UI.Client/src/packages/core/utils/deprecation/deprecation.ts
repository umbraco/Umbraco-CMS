import { umbIsProductionBuild } from '../is-production-build.function.js';
import { umbParseDeprecationOrigin, umbShouldLogDeprecation } from './deprecation-origin.js';
import type { UmbDeprecationArgs } from './types.js';

/**
 * Helper class for deprecation warnings.
 * @exports
 * @class UmbDeprecation
 */
export class UmbDeprecation {
	#messagePrefix: string = 'Umbraco Backoffice:';
	#deprecated: string;
	#removeInVersion: string;
	#solution: string;

	constructor(args: UmbDeprecationArgs) {
		this.#deprecated = args.deprecated;
		this.#removeInVersion = args.removeInVersion;
		this.#solution = args.solution;
	}

	/**
	 * Logs a warning message to the console.
	 * @memberof UmbDeprecation
	 * @param {object} [options] - Options for the warning message.
	 * @param {boolean} [options.logAlways] - If true, the warning is always logged regardless of origin (defaults to false).
	 * @returns {void}
	 * @example
	 * const deprecation = new UmbDeprecation({
	 *   deprecated: 'The "foo" function is deprecated.',
	 *   removeInVersion: '2.0.0',
	 *   solution: 'Use the "bar" function instead.'
	 * });
	 * deprecation.warn();
	 */
	warn(options: { logAlways?: boolean } = {}): void {
		const origin = umbParseDeprecationOrigin(this.#stack(), import.meta.url);
		if (!options.logAlways && !umbShouldLogDeprecation(origin, umbIsProductionBuild())) return;
		const message = `${this.#messagePrefix} ${this.#deprecated} The feature will be removed in version ${this.#removeInVersion}. ${this.#solution}`;
		console.warn(origin.type === 'unknown' ? message : `[${origin.label}] ${message}`);
	}

	#stack(): string {
		try {
			throw new Error();
		} catch (e) {
			if (e instanceof Error) {
				return e.stack ?? '';
			}
			return '';
		}
	}
}
