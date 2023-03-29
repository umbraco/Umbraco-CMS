import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { MediaTypeDetails } from '@umbraco-cms/backoffice/models';

/**
 * @export
 * @class UmbMediaTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Media Types
 */
export class UmbMediaTypeStore extends UmbStoreBase {
	#data = new ArrayState<MediaTypeDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN.toString());
	}

	append(mediaType: MediaTypeDetails) {
		this.#data.append([mediaType]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeStore>('UmbMediaTypeStore');
