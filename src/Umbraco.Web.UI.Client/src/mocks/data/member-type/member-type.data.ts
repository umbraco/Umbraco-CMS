import type { MemberTypeItemResponseModel, MemberTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

type UmbMockMemberTypeModel = MemberTypeResponseModel & MemberTypeItemResponseModel;

export const data: Array<UmbMockMemberTypeModel> = [
	{
		name: 'Media Type 1',
		id: 'media-type-1-id',
		description: 'Media type 1 description',
		alias: 'mediaType1',
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
