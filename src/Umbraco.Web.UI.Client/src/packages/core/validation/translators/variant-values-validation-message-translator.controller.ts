import type { UmbServerModelValidationContext } from '../context/server-model-validation.context.js';
import { UmbDataPathPropertyValueFilter } from '../utils/data-path-property-value-filter.function.js';
import type { UmbValidationMessageTranslator } from './validation-message-translator.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbVariantValuesValidationMessageTranslator
	extends UmbControllerBase
	implements UmbValidationMessageTranslator
{
	//
	#context: UmbServerModelValidationContext;

	constructor(host: UmbControllerHost, context: UmbServerModelValidationContext) {
		super(host);
		context.addTranslator(this);
		this.#context = context;
	}

	translate(path: string) {
		if (path.indexOf('$.values[') !== 0) {
			// We do not handle this path.
			return false;
		}
		const pathEnd = path.indexOf(']');
		if (pathEnd === -1) {
			// We do not handle this path.
			return false;
		}
		// retrieve the number from the message values index: [NL]
		const index = parseInt(path.substring(9, pathEnd));

		if (isNaN(index)) {
			// index is not a number, this means its not a path we want to translate. [NL]
			return false;
		}

		// Get the data from the validation request, the context holds that for us: [NL]
		const data = this.#context.getData();

		const specificValue = data.values[index];
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		return '$.values[' + UmbDataPathPropertyValueFilter(specificValue) + path.substring(path.indexOf(']'));
	}

	override destroy(): void {
		super.destroy();
		this.#context.removeTranslator(this);
	}
}
