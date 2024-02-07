import type { UmbTemplateDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbTemplateDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbTemplateDetailStore extends UmbDetailStoreBase<UmbTemplateDetailModel> {
	/**
	 * Creates an instance of UmbTemplateDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TEMPLATE_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_TEMPLATE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbTemplateDetailStore>('UmbTemplateDetailStore');
