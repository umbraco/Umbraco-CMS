import type { MediaTypeDetails } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbMediaTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Media Types
 */
export class UmbMediaTypeStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<MediaTypeDetails>([], (x) => x.id));
	}

	append(mediaType: MediaTypeDetails) {
		this._data.append([mediaType]);
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeStore>('UmbMediaTypeStore');
