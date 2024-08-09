import { UmbDataPathPropertyValueFilter } from '../utils/data-path-property-value-filter.function.js';
import type { UmbValidationMessageTranslator } from './validation-message-translator.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_SERVER_MODEL_VALIDATION_CONTEXT } from '../index.js';

export class UmbVariantValuesValidationMessageTranslator
	extends UmbControllerBase
	implements UmbValidationMessageTranslator
{
	//
	#context?: typeof UMB_SERVER_MODEL_VALIDATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_SERVER_MODEL_VALIDATION_CONTEXT, (context) => {
			this.#context = context;
			context.addTranslator(this);
		});
	}

	override hostDisconnected(): void {
		this.#context?.removeTranslator(this);
		this.#context = undefined;
		super.hostDisconnected();
	}

	translate(path: string) {
		if (!this.#context) return;
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

	//hostDisconnected is called when a controller is begin destroyed, so this destroy method is not needed.
	/*
	override destroy(): void {
		super.destroy();
		this.#context.removeTranslator(this);
	}*/
}
