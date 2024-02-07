import type { MemberTypeItemResponseModel, MemberTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

type UmbMockMemberTypeModel = MemberTypeResponseModel & MemberTypeItemResponseModel;

export const data: Array<UmbMockMemberTypeModel> = [
	{
		name: 'Member Type 1',
		id: 'member-type-1-id',
		description: 'Member type 1 description',
		alias: 'memberType1',
		icon: 'icon-bug',
		properties: [],
		containers: [],
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		compositions: [],
	},
];
