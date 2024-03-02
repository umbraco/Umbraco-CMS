import type {
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	MediaTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

type UmbMockMediaTypeModelHack = MediaTypeResponseModel & MediaTypeTreeItemResponseModel & MediaTypeItemResponseModel;

export interface UmbMockMediaTypeModel extends Omit<UmbMockMediaTypeModelHack, 'type'> {}

export const data: Array<UmbMockMediaTypeModel> = [
	{
		name: 'Media Type 1',
		id: 'media-type-1-id',
		parent: null,
		description: 'Media type 1 description',
		alias: 'mediaType1',
		icon: 'icon-bug',
		properties: [
			{
				id: '19',
				container: { id: 'c3cd2f12-b7c4-4206-8d8b-27c061589f75' },
				alias: 'mediaPicker',
				name: 'Media Picker',
				description: '',
				dataType: { id: 'dt-uploadField' },
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
				id: '5b4ca208-134e-4865-b423-06e5e97adf3c',
				container: { id: 'c3cd2f12-b7c4-4206-8d8b-27c061589f75' },
				alias: 'mediaType1Property1',
				name: 'Media Type 1 Property 1',
				description: null,
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
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
				id: '7',
				container: { id: 'c3cd2f12-b7c4-4206-8d8b-27c061589f75' },
				alias: 'listView',
				name: 'List View',
				description: '',
				dataType: { id: 'dt-collectionView' },
				variesByCulture: false,
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
		],
		containers: [
			{
				id: 'c3cd2f12-b7c4-4206-8d8b-27c061589f75',
				parent: null,
				name: 'Content',
				type: 'Group',
				sortOrder: 0,
			},
		],
		allowedAsRoot: true,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		allowedMediaTypes: [
			{ mediaType: { id: 'media-type-1-id' }, sortOrder: 0 },
		],
		compositions: [],
		isFolder: false,
		hasChildren: false,
		collection: { id: 'dt-collectionView' },
	},
];
