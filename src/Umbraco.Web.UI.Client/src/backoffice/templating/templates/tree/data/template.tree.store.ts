import { EntityTreeItem } from '@umbraco-cms/backend-api';
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

	appendTreeItems(items: Array<EntityTreeItem>) {
		this.#data.append(items);
	}

	updateItem(key: string, data: Partial<EntityTreeItem>) {
		const entries = this.#data.getValue();
		const entry = entries.find((entry) => entry.key === key);

		if (entry) {
			this.#data.appendOne({ ...entry, ...data });
		}
	}

	removeItem(key: string) {
		const entries = this.#data.getValue();
		const entry = entries.find((entry) => entry.key === key);

		if (entry) {
			this.#data.remove([key]);
		}
	}

	treeRootChanged() {
		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === null));
	}

	treeItemChildrenChanged(key: string) {
		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === key));
	}

	treeItemsChanged(keys: Array<string>) {
		return createObservablePart(this.#data, (items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}

export const UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateTreeStore>(
	UmbTemplateTreeStore.name
);
