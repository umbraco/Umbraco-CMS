import { DocumentTypeResource, DocumentTreeItemModel } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

export const UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeTreeStore>(
	'UmbDocumentTypeTreeStore'
);

/**
 * @export
 * @class UmbDocumentTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Data Types
 */
export class UmbDocumentTypeTreeStore extends UmbStoreBase {
	#data = new ArrayState<DocumentTreeItemModel>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDocumentTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/document-type/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.#data.remove(keys);
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, DocumentTypeResource.getTreeDocumentTypeRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		// TODO: remove ignore when we know how to handle trashed items.
		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === null));
	}

	getTreeItemChildren(key: string) {
		tryExecuteAndNotify(
			this._host,
			DocumentTypeResource.getTreeDocumentTypeChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		// TODO: remove ignore when we know how to handle trashed items.
		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === key));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				DocumentTypeResource.getTreeDocumentTypeItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
					this.#data.append(data);
				}
			});
		}

		return this.#data.getObservablePart((items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
