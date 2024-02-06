import type { MemberTypeDetails } from '../../packages/members/member-types/types.js';
import { UmbData } from './data.js';
import { createEntityTreeItem } from './utils.js';
import type {
	NamedEntityTreeItemResponseModel,
	PagedNamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<MemberTypeDetails> = [
	{
		name: 'Member Type 1',
		entityType: 'member-type',
		hasChildren: false,
		id: 'd59be02f-1df9-4228-aa1e-01917d806cda',
		parentId: null,
		alias: 'memberType1',
		properties: [],
		isFolder: false,
	},
];

// Temp mocked database
class UmbMemberTypeData extends UmbData<MemberTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedNamedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === null);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedNamedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === id);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<NamedEntityTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbMemberTypeData = new UmbMemberTypeData();
