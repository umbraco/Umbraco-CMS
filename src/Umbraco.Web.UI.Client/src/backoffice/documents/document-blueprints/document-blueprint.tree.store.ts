import { DocumentBlueprintResource, DocumentTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DocumentBlueprint_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentBlueprintTreeStore>('UmbDocumentBlueprintTreeStore');


/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintTreeStore extends UmbStoreBase {


	#data = new UniqueArrayBehaviorSubject<DocumentTreeItem>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DocumentBlueprint_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Document Blueprint Type.
	 * @param {string[]} keys
	 * @memberof UmbDocumentBlueprintsStore
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

		this.#data.remove(keys);
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, DocumentBlueprintResource.getTreeDocumentBlueprintRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === null && !item.isTrashed));
	}

	getTreeItemChildren(key: string) {
		/*
		tryExecuteAndNotify(
			this._host,
			DocumentBlueprintResource.getTreeDocumentBlueprintChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});
		*/

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === key && !item.isTrashed));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				DocumentBlueprintResource.getTreeDocumentBlueprintItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
					this.#data.append(data);
				}
			});
		}

		return createObservablePart(this.#data, (items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
