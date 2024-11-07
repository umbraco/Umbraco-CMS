import { UmbValidationPathTranslatorBase } from './validation-path-translator-base.controller.js';
import type { UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbAbstractArrayValidationPathTranslator extends UmbValidationPathTranslatorBase {
	#initialPathToMatch: string;
	#queryMethod: (data: unknown) => string;

	constructor(
		host: UmbControllerHost,
		initialPathToMatch: string,
		queryMethod: (data: any) => string,
		ctrlAlias?: UmbControllerAlias,
	) {
		super(host, ctrlAlias);

		this.#initialPathToMatch = initialPathToMatch;
		this.#queryMethod = queryMethod;
	}
	translate(path: string) {
		if (!this._context) return;
		if (path.indexOf(this.#initialPathToMatch) !== 0) {
			// We do not handle this path.
			return false;
		}
		const pathEnd = path.indexOf(']');
		if (pathEnd === -1) {
			// We do not handle this path.
			return false;
		}
		// retrieve the number from the message values index: [NL]
		const index = parseInt(path.substring(this.#initialPathToMatch.length, pathEnd));

		if (isNaN(index)) {
			// index is not a number, this means its not a path we want to translate. [NL]
			return false;
		}

		// Get the data from the validation request, the context holds that for us: [NL]
		const data = this.getDataFromIndex(index);

		if (!data) return false;
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		return this.#initialPathToMatch + this.#queryMethod(data) + path.substring(path.indexOf(']'));
	}

	abstract getDataFromIndex(index: number): unknown | undefined;
}
