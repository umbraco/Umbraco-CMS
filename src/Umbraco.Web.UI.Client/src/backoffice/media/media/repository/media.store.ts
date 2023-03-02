import type { MediaDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbMediaStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbMediaStore extends UmbStoreBase {
	#data = new ArrayState<MediaDetails>([], (x) => x.key);

	/**
	 * Creates an instance of UmbMediaStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMediaStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a media to the store
	 * @param {MediaDetails} media
	 * @memberof UmbMediaStore
	 */
	append(media: MediaDetails) {
		this.#data.append([media]);
	}

	/**
	 * Removes media in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbMediaStore
	 */
	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_MEDIA_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaStore>('UmbMediaStore');
