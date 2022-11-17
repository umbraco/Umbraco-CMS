import { UmbEntityData } from './entity.data';
import { EntityTreeItem, PagedEntityTreeItem } from '@umbraco-cms/backend-api';
import type { MemberGroupDetails } from '@umbraco-cms/models';

export const data: Array<MemberGroupDetails> = [
	{
		name: 'Member Group 1',
		type: 'member-group',
		icon: 'umb:document',
		hasChildren: false,
		key: '76708ccd-4179-464c-b694-6969149dd9f9',
		isContainer: false,
		parentKey: null,
	},
];

// Temp mocked database
class UmbMemberGroupData extends UmbEntityData<MemberGroupDetails> {
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
		return this.data.filter((item) => keys.includes(item.key));
	}
}

export const umbMemberGroupData = new UmbMemberGroupData();
