import type { PagedTagResponseModel, TagResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export const UMB_TAG_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTagStore>('UmbTAGStore');
/**
 * @export
 * @class UmbTagStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbTagStore extends UmbStoreBase {
	#data = new ArrayState<TagResponseModel>([], (x) => x.id);

	/**
	 * Creates an instance of UmbTagStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTagStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TAG_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a tag to the store
	 * @param {TagResponseModel} TAG
	 * @memberof UmbTagStore
	 */
	append(TAG: TagResponseModel) {
		this.#data.append([TAG]);
	}

	/**
	 * Append a tag to the store
	 * @param {id} TAGResponseModel id.
	 * @memberof UmbTagStore
	 */
	byId(id: TagResponseModel['id']) {
		return this.#data.getObservablePart((x) => x.find((y) => y.id === id));
	}

	/**
	 * Removes tag in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbTagStore
	 */
	remove(uniques: Array<TagResponseModel['id']>) {
		this.#data.remove(uniques);
	}
}
