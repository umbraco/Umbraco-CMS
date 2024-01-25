import type { UmbMediaDetailModel } from '../index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbMediaStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbMediaStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbMediaStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_STORE_CONTEXT.toString(), new UmbArrayState<UmbMediaDetailModel>([], (x) => x.id));
	}

	/**
	 * Retrieve a media from the store
	 * @param {string} id
	 * @memberof UmbMediaStore
	 */
	byId(id: UmbMediaDetailModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}
}

export const UMB_MEDIA_STORE_CONTEXT = new UmbContextToken<UmbMediaStore>('UmbMediaStore');
