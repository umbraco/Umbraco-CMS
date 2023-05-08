import type { TagResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export const UMB_TAG_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTagStore>('UmbTagStore');
/**
 * @export
 * @class UmbTagStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbTagStore extends UmbStoreBase {
	public readonly data = this._data.asObservable();

	/**
	 * Creates an instance of UmbTagStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTagStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TAG_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<TagResponseModel>([], (x) => x.id));
		console.log('Store is open');
	}

	/**
	 * Append a tag to the store
	 * @param {TagResponseModel} TAG
	 * @memberof UmbTagStore
	 */
	append(tag: TagResponseModel) {
		this._data.append([tag]);
	}

	/**
	 * Append a tag to the store
	 * @param {id} TagResponseModel id.
	 * @memberof UmbTagStore
	 */
	byId(id: TagResponseModel['id']) {
		return this._data.getObservablePart((x) => x.find((y) => y.id === id));
	}

	items(group: TagResponseModel['group'], culture: string) {
		return this._data.getObservablePart((items) =>
			items.filter((item) => item.group === group && item.culture === culture)
		);
	}

	//TODO Skriv god kommentar til filter/exclude

	byQuery(group: TagResponseModel['group'], culture: string, query: string) {
		return this._data.getObservablePart((items) =>
			items.filter(
				(item) =>
					item.group === group &&
					item.culture === culture &&
					item.query?.toLocaleLowerCase().includes(query.toLocaleLowerCase())
			)
		);
	}

	/**
	 * Removes tag in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbTagStore
	 */
	remove(uniques: Array<TagResponseModel['id']>) {
		this._data.remove(uniques);
	}
}
