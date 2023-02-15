import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ArrayState } from '@umbraco-cms/observable-api';
import type { MediaTypeDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbMediaTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Media Types
 */
export class UmbMediaTypeDetailStore
	extends UmbStoreBase
{
	#data = new ArrayState<MediaTypeDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UmbMediaTypeDetailStore.name);
	}

	append(mediaType: MediaTypeDetails) {
		this.#data.append([mediaType]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}	
}

export const UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeDetailStore>(
	UmbMediaTypeDetailStore.name
);