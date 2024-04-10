import type { UmbServerModelValidationContext } from '../context/server-model-validation.context.js';
import { UmbDataPathValueFilter } from '../utils/data-path-value-filter.function.js';
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

	match(message: string): boolean {
		//return message.startsWith('values[');
		// regex match, which starts with "$.values[" and then a number and then continues:
		return message.indexOf('$.values[') === 0;
	}
	translate(path: string): string {
		console.log('translate', path);

		// retrieve the number from the message values index:
		const index = parseInt(path.substring(9, path.indexOf(']')));
		//
		const data = this.#context.getData();

		const specificValue = data.values[index];
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		return '$.values[' + UmbDataPathValueFilter(specificValue) + path.substring(path.indexOf(']'));
	}

	destroy(): void {
		super.destroy();
		this.#context.removeTranslator(this);
	}
}
