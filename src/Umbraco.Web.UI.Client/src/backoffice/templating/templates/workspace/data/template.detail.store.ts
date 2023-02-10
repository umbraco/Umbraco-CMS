import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import type { TemplateModel } from '@umbraco-cms/backend-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbTemplateDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbTemplateDetailStore extends UmbStoreBase {
	#data = new ArrayState<TemplateModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbTemplateDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbTemplateDetailStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UmbTemplateDetailStore.name);
	}

	/**
	 * Append a template to the store
	 * @param {Template} template
	 * @memberof UmbTemplateDetailStore
	 */
	append(template: TemplateModel) {
		this.#data.append([template]);
	}

	/**
	 * Removes templates in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbTemplateDetailStore
	 */
	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateDetailStore>(
	UmbTemplateDetailStore.name
);
