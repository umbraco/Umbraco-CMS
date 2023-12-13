import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbScriptDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for scripts
 */
export class UmbScriptDetailStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbScriptDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbScriptDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPTS_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<TemplateResponseModel>([], (x) => x.id));
	}
}

export const UMB_SCRIPTS_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbScriptDetailStore>('UmbScriptDetailStore');
