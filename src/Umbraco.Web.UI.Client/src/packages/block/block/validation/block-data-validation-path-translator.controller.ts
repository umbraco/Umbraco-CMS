import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbValidationPathTranslatorBase } from '@umbraco-cms/backoffice/validation';
import { UmbDataPathBlockElementDataFilter } from './data-path-element-data-filter.function.js';

export class UmbBlockElementDataValidationPathTranslator extends UmbValidationPathTranslatorBase {
	#propertyName: string;
	#pathStart: string;

	constructor(host: UmbControllerHost, propertyName: 'contentData' | 'settingsData') {
		super(host);
		this.#propertyName = propertyName;
		this.#pathStart = '$.' + propertyName + '[';
	}

	translate(path: string) {
		if (!this._context) return;
		if (path.indexOf(this.#pathStart) !== 0) {
			// We do not handle this path.
			return false;
		}
		const startLength = this.#pathStart.length;
		const pathEnd = path.indexOf(']', startLength);
		if (pathEnd === -1) {
			// We do not handle this path.
			return false;
		}
		// retrieve the number from the message values index: [NL]
		const index = parseInt(path.substring(startLength, pathEnd));

		if (isNaN(index)) {
			// index is not a number, this means its not a path we want to translate. [NL]
			return false;
		}

		// Get the data from the validation request, the context holds that for us: [NL]
		const data = this._context.getTranslationData();

		const specificValue = data[this.#propertyName][index];
		if (!specificValue || !specificValue.udi) {
			console.log('block did not have UDI', this.#propertyName, index, data);
			return false;
		}
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		return this.#pathStart + UmbDataPathBlockElementDataFilter(specificValue) + path.substring(path.indexOf(']'));
	}
}
