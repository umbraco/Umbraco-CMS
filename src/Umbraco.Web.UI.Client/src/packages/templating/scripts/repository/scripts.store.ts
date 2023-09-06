import { UMB_SCRIPTS_STORE_CONTEXT_TOKEN_ALIAS } from '../config.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbScriptsStore
 * @extends {UmbStoreBase}
 * @description - Data Store for scripts
 */
export class UmbScriptsStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbScriptsStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbScriptsStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPTS_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<TemplateResponseModel>([], (x) => x.id));
	}

	/**
	 * Append a script to the store
	 * @param {Template} template
	 * @memberof UmbScriptsStore
	 */
	append(template: TemplateResponseModel) {
		this._data.append([template]);
	}

	/**
	 * Removes scripts in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbScriptsStore
	 */
	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_SCRIPTS_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbScriptsStore>(
	UMB_SCRIPTS_STORE_CONTEXT_TOKEN_ALIAS,
);
