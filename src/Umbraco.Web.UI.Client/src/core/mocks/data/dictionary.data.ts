import type { DictionaryDetails } from '../../../backoffice/translation/dictionary';
import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<DictionaryDetails> = [
	{
		$type: '',
		parentKey: null,
		name: 'Hello',
		key: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		hasChildren: true,
		type: 'dictionary-item',
		isContainer: false,
		icon: 'umb:book-alt',
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
		$type: '',
		parentKey: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		name: 'Hello again',
		key: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		hasChildren: false,
		type: 'dictionary-item',
		isContainer: false,
		icon: 'umb:book-alt',
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

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDictionaryData extends UmbEntityData<DictionaryDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<EntityTreeItemResponseModel> {
		const rootItems = this.data.filter((item) => item.parentKey === null);
		return rootItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<EntityTreeItemResponseModel> {
		const childItems = this.data.filter((item) => item.parentKey === key);
		return childItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItemResponseModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbDictionaryData = new UmbDictionaryData();
