import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbTemplateStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Templates
 */
export class UmbTemplateStore extends UmbStoreBase {
	#data = new ArrayState<TemplateResponseModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbTemplateStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TEMPLATE_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a template to the store
	 * @param {Template} template
	 * @memberof UmbTemplateStore
	 */
	append(template: TemplateResponseModel) {
		this.#data.append([template]);
	}

	/**
	 * Removes templates in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbTemplateStore
	 */
	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_TEMPLATE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateStore>('UmbTemplateStore');
