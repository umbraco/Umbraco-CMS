import type { MemberDetails } from '../../packages/members/members/types';
import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import type {
	EntityTreeItemResponseModel,
	PagedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<MemberDetails> = [
	{
		$type: '',
		name: 'Member AAA',
		type: 'member',
		icon: 'umb:user',
		hasChildren: false,
		id: 'aaa08ccd-4179-464c-b634-6969149dd9f9',
		isContainer: false,
		parentId: null,
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

export const umbMemberData = new UmbMemberData();
