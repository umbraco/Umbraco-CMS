import { UmbData } from './data';
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
		isTrashed: false,
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
		const total = items.length;
		return { items, total };
	}

	getTreeItemChildren(key: string): PagedEntityTreeItem {
		const items = this.data.filter((item) => item.parentKey === key);
		const total = items.length;
		return { items, total };
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItem> {
		return this.data.filter((item) => keys.includes(item.key ?? ''));
	}
}

export const umbMemberTypeData = new UmbMemberTypeData();
