import { FolderTreeItemModel, MediaTypeResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

export const UMB_DATA_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeTreeStore>(
	'UmbMediaTypeTreeStore'
);

/**
 * @export
 * @class UmbMediaTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Media Types
 */
export class UmbMediaTypeTreeStore extends UmbStoreBase {
	#data = new ArrayState<FolderTreeItemModel>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DATA_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, MediaTypeResource.getTreeMediaTypeRoot({})).then(({ data }) => {
			if (data) {
				this.#data.append(data.items);
			}
		});

		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === null));
	}

	getTreeItemChildren(key: string) {
		tryExecuteAndNotify(
			this._host,
			MediaTypeResource.getTreeMediaTypeChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				this.#data.append(data.items);
			}
		});

		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === key));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				MediaTypeResource.getTreeMediaTypeItem({
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
