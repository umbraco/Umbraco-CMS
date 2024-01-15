import { UmbMediaTypeDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbMediaTypeStore
 * @extends {UmbDetailStoreBase}
 * @description - Data Store for Media Types
 */
export class UmbMediaTypeDetailStore extends UmbDetailStoreBase<UmbMediaTypeDetailModel> {
	/**
	 * Creates an instance of UmbMediaTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeDetailStore>(
	'UmbMediaTypeDetailStore',
);
