import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @export
 * @class UmbMediaTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Media Types
 */
export class UmbMediaTypeStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<MediaTypeResponseModel>([], (x) => x.id),
		);
	}

	byId(id: MediaTypeResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}

	append(mediaType: MediaTypeResponseModel) {
		this._data.append([mediaType]);
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeStore>('UmbMediaTypeStore');
