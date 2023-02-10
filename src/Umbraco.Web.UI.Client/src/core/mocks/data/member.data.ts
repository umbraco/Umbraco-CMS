import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import type { MemberDetails } from '@umbraco-cms/models';
import { EntityTreeItem, PagedEntityTreeItem } from '@umbraco-cms/backend-api';

export const data: Array<MemberDetails> = [
	{
		name: 'Member AAA',
		type: 'member',
		icon: 'umb:user',
		hasChildren: false,
		key: 'aaa08ccd-4179-464c-b634-6969149dd9f9',
		isContainer: false,
		parentKey: null,
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbMemberData extends UmbEntityData<MemberDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedEntityTreeItem {
		const items = this.data.filter((item) => item.parentKey === null);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(key: string): PagedEntityTreeItem {
		const items = this.data.filter((item) => item.parentKey === key);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItem> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbMemberData = new UmbMemberData();
