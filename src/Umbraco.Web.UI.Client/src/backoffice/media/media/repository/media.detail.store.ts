import type { MediaDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbMediaDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbMediaDetailStore extends UmbStoreBase {
	#data = new ArrayState<MediaDetails>([], (x) => x.key);

	/**
	 * Creates an instance of UmbMediaDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMediaDetailStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UmbMediaDetailStore.name);
	}

	/**
	 * Append a media to the store
	 * @param {MediaDetails} media
	 * @memberof UmbMediaDetailStore
	 */
	append(media: MediaDetails) {
		this.#data.append([media]);
	}

	/**
	 * Removes media in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbMediaDetailStore
	 */
	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaDetailStore>(UmbMediaDetailStore.name);
