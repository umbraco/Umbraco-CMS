import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbDataPathPropertyValueQuery } from '../utils/data-path-property-value-query.function.js';
import { UmbAbstractArrayValidationPathTranslator } from './abstract-array-path-translator.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbVariantValuesValidationPathTranslator extends UmbAbstractArrayValidationPathTranslator {
	constructor(host: UmbControllerHost) {
		super(host, '$.values[', UmbDataPathPropertyValueQuery);

		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: 'UmbVariantValuesValidationPathTranslator',
			solution: 'UmbVariantValuesValidationPathTranslator is deprecated.',
		}).warn();
	}

	getDataFromIndex(index: number) {
		if (!this._context) return;
		const data = this._context.getTranslationData();
		return data.values[index];
	}
}
