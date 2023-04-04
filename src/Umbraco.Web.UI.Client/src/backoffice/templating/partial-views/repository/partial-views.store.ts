import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UMB_PARTIAL_VIEW_STORE_CONTEXT_TOKEN_ALIAS } from '../config';

/**
 * @export
 * @class UmbPartialViewsStore
 * @extends {UmbStoreBase}
 * @description - Data Store for partial views
 */
export class UmbPartialViewsStore extends UmbStoreBase {
	#data = new ArrayState<TemplateResponseModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbPartialViewsStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewsStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_PARTIAL_VIEWS_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a partial view to the store
	 * @param {Template} template
	 * @memberof UmbPartialViewsStore
	 */
	append(template: TemplateResponseModel) {
		this.#data.append([template]);
	}

	/**
	 * Removes partial views in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbPartialViewsStore
	 */
	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_PARTIAL_VIEWS_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbPartialViewsStore>(
	UMB_PARTIAL_VIEW_STORE_CONTEXT_TOKEN_ALIAS
);
