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
				id: '5b4ca208-134e-4865-b423-06e5e97adf3c',
				container: { id: 'c3cd2f12-b7c4-4206-8d8b-27c061589f75' },
				alias: 'blogPostText',
				name: 'Blog Post Text',
				description: null,
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: true,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'ef7096b6-7c9e-49ba-8d49-395111e65ea2',
				container: { id: '227d6ed2-e118-4494-b8f2-deb69854a56a' },
				alias: 'blogTextStringUnderMasterTab',
				name: 'Blog text string under master tab',
				description: null,
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 1,
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
			{
				id: 'e010c429-b298-499a-9bfe-79687af8972a',
				container: { id: '22177c49-ecba-4f2e-b7fa-3f2c04d02cfb' },
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				name: 'Blog text string under group under master tab',
				description: null,
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 2,
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
			{
				id: '1a22749a-c7d2-44bb-b36b-c977c2ced6ef',
				container: { id: '2c943997-b685-432d-a6c5-601d8e7a298a' },
				alias: 'localBlogTabString',
				name: 'Local Blog Tab String',
				description: null,
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: true,
					mandatoryMessage: null,
					regEx: '^[0-9]*$',
					regExMessage: null,
				},
				appearance: {
					labelOnTop: true,
				},
			},
			{
				id: '22',
				container: { id: '2c943997-b685-432d-a6c5-601d8e7a298a' },
				alias: 'blockGrid',
				name: 'Block Grid',
				description: '',
				dataType: { id: 'dt-blockGrid' },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: true,
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
				id: 'c3cd2f12-b7c4-4206-8d8b-27c061589f75',
				parent: null,
				name: 'Content-group',
				type: 'Group',
				sortOrder: 0,
			},
			{
				id: '227d6ed2-e118-4494-b8f2-deb69854a56a',
				parent: null,
				name: 'Master Tab',
				type: 'Tab',
				sortOrder: 0,
			},
			{
				id: '22177c49-ecba-4f2e-b7fa-3f2c04d02cfb',
				parent: { id: '227d6ed2-e118-4494-b8f2-deb69854a56a' },
				name: 'Blog Group under master tab',
				type: 'Group',
				sortOrder: 0,
			},
			{
				id: '2c943997-b685-432d-a6c5-601d8e7a298a',
				parent: null,
				name: 'Local blog tab',
				type: 'Tab',
				sortOrder: 1,
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
