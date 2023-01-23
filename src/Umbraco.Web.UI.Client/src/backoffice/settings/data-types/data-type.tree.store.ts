import { DataTypeResource, DocumentTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentTreeStore');


/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbStoreBase<DocumentTree>}
 * @description - Tree Data Store for Data Types
 */
export class UmbDocumentTreeStore extends UmbStoreBase {


	private _data = new UniqueArrayBehaviorSubject<DocumentTreeItem>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDataTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/data-type/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this._data.remove(keys);
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, DataTypeResource.getTreeDataTypeRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this._data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this._data, (items) => items.filter((item) => item.parentKey === null && !item.isTrashed));
	}

	getTreeItemChildren(key: string) {
		tryExecuteAndNotify(
			this._host,
			DataTypeResource.getTreeDataTypeChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this._data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this._data, (items) => items.filter((item) => item.parentKey === key && !item.isTrashed));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				DataTypeResource.getTreeDataTypeItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
					this._data.append(data);
				}
			});
		}

		return createObservablePart(this._data, (items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
