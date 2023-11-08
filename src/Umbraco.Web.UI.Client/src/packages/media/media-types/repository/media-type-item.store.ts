import { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbMediaTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Media Type items
 */

export class UmbMediaTypeItemStore
	extends UmbStoreBase<MediaTypeItemResponseModel>
	implements UmbItemStore<MediaTypeItemResponseModel>
{
	/**
	 * Creates an instance of UmbMediaTypeItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<MediaTypeItemResponseModel>([], (x) => x.id),
		);
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeItemStore>(
	'UmbMediaTypeItemStore',
);
