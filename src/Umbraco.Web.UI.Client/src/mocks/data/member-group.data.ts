import { UmbEntityData } from './entity.data.js';
import { createEntityTreeItem } from './utils.js';
import type {
	EntityTreeItemResponseModel,
	PagedNamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbMemberGroupDetailModel } from 'src/packages/members/member-group/index.js';

export const data: Array<any> = [
	{
		name: 'Member Group AAA',
		type: 'member-group',
		hasChildren: false,
		id: '76708ccd-4179-464c-b694-6969149dd9f9',
		isContainer: false,
		parentId: null,
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbMemberGroupData extends UmbEntityData<UmbMemberGroupDetailModel> {
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

export const umbMemberGroupData = new UmbMemberGroupData();
