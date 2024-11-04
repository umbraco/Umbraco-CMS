import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbAbstractArrayValidationPathTranslator,
	UmbDataPathPropertyValueQuery,
} from '@umbraco-cms/backoffice/validation';

export class UmbBlockElementValuesDataValidationPathTranslator extends UmbAbstractArrayValidationPathTranslator {
	constructor(host: UmbControllerHost) {
		super(host, '$.values[', UmbDataPathPropertyValueQuery);
	}

	getDataFromIndex(index: number) {
		if (!this._context) return;
		const data = this._context.getTranslationData();
		return data.values[index];
	}
}
