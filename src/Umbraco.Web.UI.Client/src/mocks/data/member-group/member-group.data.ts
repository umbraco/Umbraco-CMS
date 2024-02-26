import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberGroupModel = MemberGroupItemResponseModel;

export const data: Array<UmbMockMemberGroupModel> = [
	{
		name: 'Member Group 1',
		id: 'member-group-1-id',
	},
	{
		name: 'Member Group 2',
		id: 'member-group-2-id',
	},
];
