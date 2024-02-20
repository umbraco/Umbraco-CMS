import type {
	MemberTypeItemResponseModel,
	MemberTypeResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberTypeModel = MemberTypeResponseModel &
	MemberTypeItemResponseModel & {
		hasChildren: boolean;
		parent: { id: string } | null;
		hasListView: boolean;
	};

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
		parent: null,
		hasChildren: false,
		hasListView: false,
	},
];
