import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberGroupModel = MemberGroupItemResponseModel;

export const data: Array<UmbMockMemberGroupModel> = [
	{
		name: 'Member Group 1',
		id: 'member-group-1-id',
		signs: [],
	},
	{
		name: 'Member Group 2',
		id: 'member-group-2-id',
		signs: [],
	},
	{
		name: 'Forbidden Member Group',
		id: 'forbidden',
		signs: [],
	},
];
