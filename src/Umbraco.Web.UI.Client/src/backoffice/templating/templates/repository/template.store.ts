import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import type { TemplateModel } from '@umbraco-cms/backend-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbTemplateStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Templates
 */
export class UmbTemplateStore extends UmbStoreBase {
	#data = new ArrayState<TemplateModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbTemplateStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbTemplateStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_TEMPLATE_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a template to the store
	 * @param {Template} template
	 * @memberof UmbTemplateStore
	 */
	append(template: TemplateModel) {
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
