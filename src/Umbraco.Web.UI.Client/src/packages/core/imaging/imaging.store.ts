import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';

/**
 * @export
 * @class UmbImagingStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Imaging Items
 */

export class UmbImagingStore extends UmbItemStoreBase<UmbMediaUrlModel> {
	/**
	 * Creates an instance of UmbImagingStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbImagingStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_IMAGING_STORE_CONTEXT.toString());
	}
}

export default UmbImagingStore;

export const UMB_IMAGING_STORE_CONTEXT = new UmbContextToken<UmbImagingStore>('UmbImagingStore');
