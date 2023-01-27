import { DocumentResource, DocumentTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase, UmbTreeStore } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentTreeStore');


/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Documents
 */
export class UmbDocumentTreeStore extends UmbStoreBase implements UmbTreeStore<DocumentTreeItem> {


	private _data = new ArrayState<DocumentTreeItem>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	// TODO: how do we handle trashed items?
	async trash(keys: Array<string>) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/document/trash', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this._data.append(data);
	}

	async move(keys: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/document/move', {
			method: 'POST',
			body: JSON.stringify({ keys, destination }),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this._data.append(data);
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, DocumentResource.getTreeDocumentRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this._data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return this._data.getObservablePart((items) => items.filter((item) => item.parentKey === null && !item.isTrashed));
	}

	getTreeItemChildren(key: string) {
		tryExecuteAndNotify(
			this._host,
			DocumentResource.getTreeDocumentChildren({
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
		return this._data.getObservablePart((items) => items.filter((item) => item.parentKey === key && !item.isTrashed));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				DocumentResource.getTreeDocumentItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
					this._data.append(data);
				}
			});
		}

		return this._data.getObservablePart((items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
