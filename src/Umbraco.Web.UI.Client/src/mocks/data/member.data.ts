import type { UmbMemberDetailModel } from '../../packages/members/members/types.js';
import { UmbEntityData } from './entity.data.js';
import { createEntityTreeItem } from './utils.js';
import type {
	EntityTreeItemResponseModel,
	PagedNamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<UmbMemberDetailModel> = [
	{
		name: 'Member AAA',
		entityType: 'member',
		hasChildren: false,
		id: 'aaa08ccd-4179-464c-b634-6969149dd9f9',
		isContainer: false,
		parentId: null,
		isFolder: false,
	},
];

class UmbMemberData extends UmbEntityData<UmbMemberDetailModel> {
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

	getTreeItem(ids: Array<string>): Array<EntityTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbMemberData = new UmbMemberData();
