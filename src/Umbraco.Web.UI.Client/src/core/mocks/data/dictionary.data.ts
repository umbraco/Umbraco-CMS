import type { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import type { EntityTreeItemModel } from '@umbraco-cms/backend-api';
import type { DictionaryDetails } from '@umbraco-cms/models';

export const data: Array<DictionaryDetails> = [
	{
		parentKey: null,
		name: 'Hello',
		key: 'b7e7d0ab-53ba-485d-b8bd-12537f9925cb',
		hasChildren: true,
		type: 'dictionary',
		isContainer: false,
		icon: 'umb:icon-book-alt',
	},
	{
		parentKey: 'b7e7d0ab-53ba-485d-b8bd-12537f9925cb',
		name: 'Hello',
		key: 'b7e7d0ab-53bb-485d-b8bd-12537f9925cb',
		hasChildren: false,
		type: 'dictionary',
		isContainer: false,
		icon: 'umb:icon-book-alt',
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDictionaryData extends UmbEntityData<DictionaryDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<EntityTreeItemModel> {
		const rootItems = this.data.filter((item) => item.parentKey === null);
		return rootItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<EntityTreeItemModel> {
		const childItems = this.data.filter((item) => item.parentKey === key);
		return childItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItemModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbDictionaryData = new UmbDictionaryData();
