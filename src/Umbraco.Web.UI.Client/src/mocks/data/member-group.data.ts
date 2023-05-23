import type { MemberGroupDetails } from '../../packages/members/member-groups/types';
import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<MemberGroupDetails> = [
	{
		$type: '',
		name: 'Member Group AAA',
		type: 'member-group',
		icon: 'umb:document',
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
class UmbMemberGroupData extends UmbEntityData<MemberGroupDetails> {
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

export const umbMemberGroupData = new UmbMemberGroupData();
