import { UmbDataPathVariantQuery } from '../utils/data-path-variant-query.function.js';
import { UmbAbstractArrayValidationPathTranslator } from './abstract-array-path-translator.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbVariantsValidationPathTranslator extends UmbAbstractArrayValidationPathTranslator {
	constructor(host: UmbControllerHost) {
		super(host, '$.variants[', UmbDataPathVariantQuery);
	}

	getDataFromIndex(index: number) {
		if (!this._context) return;
		const data = this._context.getTranslationData();
		return data.variants[index];
	}
}
