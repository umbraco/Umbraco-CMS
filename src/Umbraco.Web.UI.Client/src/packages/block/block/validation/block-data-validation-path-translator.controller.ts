import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbValidationPathTranslatorBase } from '@umbraco-cms/backoffice/validation';
import { UmbDataPathBlockElementDataFilter } from './data-path-element-data-filter.function.js';

export class UmbVariantValuesValidationPathTranslator extends UmbValidationPathTranslatorBase {
	#pathStart: string;

	constructor(host: UmbControllerHost, baseDataPath: string, propertyName: 'contentData' | 'settingsData') {
		super(host);
		this.#pathStart = baseDataPath + '.' + propertyName + '[';
		console.log('UmbVariantValuesValidationPathTranslator', this.#pathStart);
	}

	translate(path: string) {
		if (!this._context) return;
		console.log('translate', path);
		if (path.indexOf(this.#pathStart) !== 0) {
			// We do not handle this path.
			return false;
		}
		const startLength = this.#pathStart.length;
		console.log('translate got a match on step one');
		const pathEnd = path.indexOf(']', startLength);
		if (pathEnd === -1) {
			// We do not handle this path.
			return false;
		}
		// retrieve the number from the message values index: [NL]
		const index = parseInt(path.substring(startLength, pathEnd));

		console.log('translate index', path.substring(startLength, pathEnd), index);
		if (isNaN(index)) {
			// index is not a number, this means its not a path we want to translate. [NL]
			return false;
		}

		// Get the data from the validation request, the context holds that for us: [NL]
		const data = this._context.getData();
		console.log('go to this point', data);

		const specificValue = data.contentData[index];
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		return this.#pathStart + UmbDataPathBlockElementDataFilter(specificValue) + path.substring(path.indexOf(']'));
	}
}
