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
		properties: [
			{
				id: '1680d4d2-cda8-4ac2-affd-a69fc10382b1',
				container: { id: 'the-simplest-document-type-id-container' },
				alias: 'prop1',
				name: 'Prop 1',
				description: null,
				isSensitive: false,
				visibility: { memberCanEdit: true, memberCanView: true },
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'the-simplest-document-type-id-container',
				parent: null,
				name: 'Content',
				type: 'Group',
				sortOrder: 0,
			},
		],
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
