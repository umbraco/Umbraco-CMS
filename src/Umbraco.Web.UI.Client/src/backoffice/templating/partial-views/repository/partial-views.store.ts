import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UMB_PARTIAL_VIEW_STORE_CONTEXT_TOKEN_ALIAS } from '../config';

/**
 * @export
 * @class UmbPartialViewsStore
 * @extends {UmbStoreBase}
 * @description - Data Store for partial views
 */
export class UmbPartialViewsStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbPartialViewsStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewsStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_PARTIAL_VIEWS_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<TemplateResponseModel>([], (x) => x.id)
		);
	}

	/**
	 * Append a partial view to the store
	 * @param {Template} template
	 * @memberof UmbPartialViewsStore
	 */
	append(template: TemplateResponseModel) {
		this._data.append([template]);
	}

	/**
	 * Removes partial views in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbPartialViewsStore
	 */
	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_PARTIAL_VIEWS_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbPartialViewsStore>(
	UMB_PARTIAL_VIEW_STORE_CONTEXT_TOKEN_ALIAS
);
