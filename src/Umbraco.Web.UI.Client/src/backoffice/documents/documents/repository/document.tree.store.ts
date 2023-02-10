import { EntityTreeItemModel } from '@umbraco-cms/backend-api';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbDocumentTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Templates
 */
// TODO: consider if tree store could be turned into a general EntityTreeStore class?
export class UmbDocumentTreeStore extends UmbStoreBase {
	#data = new ArrayState<EntityTreeItemModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbDocumentTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Appends items to the store
	 * @param {Array<EntityTreeItemModel>} items
	 * @memberof UmbDocumentTreeStore
	 */
	appendItems(items: Array<EntityTreeItemModel>) {
		this.#data.append(items);
	}

	/**
	 * Updates an item in the store
	 * @param {string} key
	 * @param {Partial<EntityTreeItemModel>} data
	 * @memberof UmbDocumentTreeStore
	 */
	updateItem(key: string, data: Partial<EntityTreeItemModel>) {
		const entries = this.#data.getValue();
		const entry = entries.find((entry) => entry.key === key);

		if (entry) {
			this.#data.appendOne({ ...entry, ...data });
		}
	}

	/**
	 * Removes an item from the store
	 * @param {string} key
	 * @memberof UmbDocumentTreeStore
	 */
	removeItem(key: string) {
		const entries = this.#data.getValue();
		const entry = entries.find((entry) => entry.key === key);

		if (entry) {
			this.#data.remove([key]);
		}
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbDocumentTreeStore
	 */
	rootItems = this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === null));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbDocumentTreeStore
	 */
	childrenOf(parentKey: string | null) {
		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === parentKey));
	}

	/**
	 * Returns an observable to observe the items with the given keys
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof UmbDocumentTreeStore
	 */
	items(keys: Array<string>) {
		return this.#data.getObservablePart((items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}

export const UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTreeStore>(
	UmbDocumentTreeStore.name
);
