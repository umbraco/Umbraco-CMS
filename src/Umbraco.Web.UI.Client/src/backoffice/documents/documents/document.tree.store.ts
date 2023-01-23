import type { Observable } from 'rxjs';
import { DocumentResource, DocumentTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentDetailStore');


/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbStoreBase<DocumentTree>}
 * @description - Data Store for Documents
 */
export class UmbDocumentTreeStore extends UmbStoreBase {


	private _data = new UniqueArrayBehaviorSubject<DocumentTreeItem>([], (x) => x.key);


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

	getTreeRoot(): Observable<Array<DocumentTreeItem>> {
		tryExecuteAndNotify(this._host, DocumentResource.getTreeDocumentRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this._data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this._data, (items) => items.filter((item) => item.parentKey === null && !item.isTrashed));
	}

	getTreeItemChildren(key: string): Observable<Array<DocumentTreeItem>> {
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
		return createObservablePart(this._data, (items) => items.filter((item) => item.parentKey === key && !item.isTrashed));
	}

	getTreeItems(keys: Array<string>): Observable<Array<DocumentTreeItem>> {
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

		return createObservablePart(this._data, (items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
