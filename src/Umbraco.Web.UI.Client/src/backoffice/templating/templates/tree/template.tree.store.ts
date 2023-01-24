import { EntityTreeItem, TemplateResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbTemplateTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Templates
 */
export class UmbTemplateTreeStore extends UmbStoreBase {
	#data = new ArrayState<EntityTreeItem>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, TemplateResource.getTreeTemplateRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === null));
	}

	getTreeItemChildren(key: string) {
		tryExecuteAndNotify(
			this._host,
			TemplateResource.getTreeTemplateChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === key));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				TemplateResource.getTreeTemplateItem({
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

export const UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateTreeStore>(
	UmbTemplateTreeStore.name
);
