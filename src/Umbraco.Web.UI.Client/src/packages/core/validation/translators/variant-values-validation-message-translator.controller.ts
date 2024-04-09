import type { UmbValidationMessageTranslator } from '../interfaces/validation-message-translator.interface.js';
import { UmbDataPathValueFilter } from '../utils/data-path-value-filter.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbVariantDatasetWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export class UmbVariantValuesValidationMessageTranslator
	extends UmbControllerBase
	implements UmbValidationMessageTranslator
{
	//
	#workspace: UmbVariantDatasetWorkspaceContext;

	constructor(host: UmbControllerHost, workspaceContext: UmbVariantDatasetWorkspaceContext) {
		super(host);
		this.#workspace = workspaceContext;
	}

	match(message: string): boolean {
		//return message.startsWith('values[');
		// regex match, for "values[" and then a number:
		return /^values\[\d+\]/.test(message);
	}
	translate(message: string): string {
		/*
		// retrieve the number from the message values index:
		const index = parseInt(message.substring(7, message.indexOf(']')));
		//
		this.#workspace.getCurrentData();
		// replace the values[ number ] with values [ number + 1 ], continues by the rest of the path:
		return 'values[' + UmbDataPathValueFilter() + message.substring(message.indexOf(']'));
		*/
		return 'not done';
	}
}
