import { UmbData } from './data';
import { createEntityTreeItem } from './utils';
import { EntityTreeItem, PagedEntityTreeItem } from '@umbraco-cms/backend-api';
import type { MemberTypeDetails } from '@umbraco-cms/models';

export const data: Array<MemberTypeDetails> = [
	{
		name: 'Member Type 1',
		type: 'member-type',
		icon: 'icon-user',
		hasChildren: false,
		key: 'd59be02f-1df9-4228-aa1e-01917d806cda',
		isContainer: false,
		parentKey: null,
		alias: 'memberType1',
		properties: [],
	},
];

// Temp mocked database
class UmbMemberTypeData extends UmbData<MemberTypeDetails> {
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

export const umbMemberTypeData = new UmbMemberTypeData();
