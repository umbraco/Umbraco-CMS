import { UmbEntityData } from './entity.data.js';
import { createEntityTreeItem } from './utils.js';
import type {
	DictionaryItemResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<DictionaryItemResponseModel> = [
	{
		name: 'Hello',
		id: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		translations: [
			{
				isoCode: 'en',
				translation: 'hello in en-US',
			},
			{
				isoCode: 'fr',
				translation: '',
			},
		],
	},
	{
		name: 'Hello again',
		id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		translations: [
			{
				isoCode: 'en',
				translation: 'Hello again in en-US',
			},
			{
				isoCode: 'fr',
				translation: 'Hello in fr',
			},
		],
	},
];

const dictionaryTree: Array<NamedEntityTreeItemResponseModel> = [
	{
		parent: null,
		name: 'Hello',
		id: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		hasChildren: true,
		type: 'dictionary-item',
	},
	{
		parent: null,
		name: 'Hello again',
		id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		hasChildren: false,
		type: 'dictionary-item',
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDictionaryData extends UmbEntityData<DictionaryItemResponseModel> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<NamedEntityTreeItemResponseModel> {
		const rootItems = dictionaryTree.filter((item) => item.parent === null);
		return rootItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItemChildren(id: string): Array<NamedEntityTreeItemResponseModel> {
		const childItems = dictionaryTree.filter((item) => item.parent?.id === id);
		return childItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItem(ids: Array<string>): Array<NamedEntityTreeItemResponseModel> {
		const items = dictionaryTree.filter((item) => ids.includes(item.id));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbDictionaryData = new UmbDictionaryData();
