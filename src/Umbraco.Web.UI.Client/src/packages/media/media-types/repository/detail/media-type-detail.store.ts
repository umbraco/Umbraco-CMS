import { MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbMediaTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Media Types
 */
export class UmbMediaTypeDetailStore extends UmbStoreBase<MediaTypeResponseModel> {
	/**
	 * Creates an instance of UmbMediaTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT.toString(),
			new UmbArrayState<MediaTypeResponseModel>([], (x) => x.id),
		);
	}

	/**
	 * @param {MediaTypeResponseModel['id']} id
	 * @return {*}
	 * @memberof UmbMediaTypeDetailStore
	 */
	byId(id: MediaTypeResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}
}

export const UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeDetailStore>(
	'UmbMediaTypeDetailStore',
);
