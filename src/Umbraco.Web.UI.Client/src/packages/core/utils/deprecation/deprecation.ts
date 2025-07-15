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
	 */
	warn() {
		console.warn(
			`${this.#messagePrefix} ${this.#deprecated} The feature will be removed in version ${this.#removeInVersion}. ${this.#solution}`,
		);
	}
}
