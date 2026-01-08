import { UmbDataPathBlockElementDataQuery } from './data-path-element-data-query.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbAbstractArrayValidationPathTranslator } from '@umbraco-cms/backoffice/validation';

export class UmbBlockElementDataValidationPathTranslator extends UmbAbstractArrayValidationPathTranslator {
	#propertyName: string;

	constructor(host: UmbControllerHost, propertyName: 'contentData' | 'settingsData') {
		super(host, '$.' + propertyName + '[', UmbDataPathBlockElementDataQuery);
		this.#propertyName = propertyName;

		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: 'UmbBlockElementDataValidationPathTranslator',
			solution: 'UmbBlockElementDataValidationPathTranslator is deprecated.',
		}).warn();
	}

	getDataFromIndex(index: number) {
		if (!this._context) return;
		const data = this._context.getTranslationData();
		const entry = data[this.#propertyName][index];
		if (!entry || !entry.key) {
			console.error('block did not have key', `${this.#propertyName}[${index}]`, entry);
			return false;
		}
		return entry;
	}
}
