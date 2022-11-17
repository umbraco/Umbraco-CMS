import { UmbData } from './data';
import { EntityTreeItem } from '@umbraco-cms/backend-api';
import type { MemberTypeDetails } from '@umbraco-cms/models';

export const data: Array<MemberTypeDetails> = [
	{
		key: 'd59be02f-1df9-4228-aa1e-01917d806cda',
		isContainer: false,
		parentKey: null,
		name: 'Member',
		type: 'member-type',
		icon: 'icon-user',
		hasChildren: false,
		alias: 'memberType1',
		properties: [],
	},
];

// Temp mocked database
class UmbMemberTypeData extends UmbData<MemberTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<EntityTreeItem> {
		return this.data.filter((item) => item.parentKey === null);
	}

	getTreeItemChildren(key: string): Array<EntityTreeItem> {
		return this.data.filter((item) => item.parentKey === key);
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItem> {
		return this.data.filter((item) => keys.includes(item.key ?? ''));
	}
}

export const umbMemberTypeData = new UmbMemberTypeData();
