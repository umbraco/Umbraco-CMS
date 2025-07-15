import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import {
	UmbAbstractArrayValidationPathTranslator,
	UmbDataPathPropertyValueQuery,
} from '@umbraco-cms/backoffice/validation';

const ctrlAlias = Symbol();

export class UmbBlockElementValuesDataValidationPathTranslator extends UmbAbstractArrayValidationPathTranslator {
	constructor(host: UmbControllerHost) {
		super(host, '$.values[', UmbDataPathPropertyValueQuery, ctrlAlias);

		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: 'UmbBlockElementValuesDataValidationPathTranslator',
			solution: 'UmbBlockElementValuesDataValidationPathTranslator is deprecated.',
		}).warn();
	}

	getDataFromIndex(index: number) {
		if (!this._context) return;
		const data = this._context.getTranslationData();
		return data.values[index];
	}
}
