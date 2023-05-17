import type { MemberTypeDetails } from '../../../packages/members/member-types/types';
import { UmbData } from './data';
import { createEntityTreeItem } from './utils';
import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<MemberTypeDetails> = [
	{
		$type: '',
		name: 'Member Type 1',
		type: 'member-type',
		icon: 'icon-user',
		hasChildren: false,
		id: 'd59be02f-1df9-4228-aa1e-01917d806cda',
		isContainer: false,
		parentId: null,
		alias: 'memberType1',
		properties: [],
	},
];

// Temp mocked database
class UmbMemberTypeData extends UmbData<MemberTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === null);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === id);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<EntityTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbMemberTypeData = new UmbMemberTypeData();
