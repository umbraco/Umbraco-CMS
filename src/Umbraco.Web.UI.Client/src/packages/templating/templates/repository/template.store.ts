import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbTemplateStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Templates
 */
export class UmbTemplateStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbTemplateStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TEMPLATE_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<TemplateResponseModel>([], (x) => x.id));
	}

	/**
	 * Append a template to the store
	 * @param {Template} template
	 * @memberof UmbTemplateStore
	 */
	append(template: TemplateResponseModel) {
		this._data.append([template]);
	}

	/**
	 * Retrieve a template from the store
	 * @param {string} id
	 * @memberof UmbTemplateStore
	 */
	byId(id: TemplateResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}

	items(uniques: string[]) {
		return this._data.asObservablePart((x) => x.filter((y) => uniques.includes(y.id)));
	}

	/**
	 * Removes templates in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbTemplateStore
	 */
	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_TEMPLATE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateStore>('UmbTemplateStore');
