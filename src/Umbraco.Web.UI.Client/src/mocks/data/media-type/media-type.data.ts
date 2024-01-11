import {
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	MediaTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export type UmbMockMediaTypeModelHack = MediaTypeResponseModel &
	MediaTypeTreeItemResponseModel &
	MediaTypeItemResponseModel;

export interface UmbMockMediaTypeModel extends Omit<UmbMockMediaTypeModelHack, 'type'> {}

export const data: Array<UmbMockMediaTypeModel> = [
	{
		name: 'Media Type 1',
		id: 'c5159663-eb82-43ee-bd23-e42dc5e71db6',
		parentId: null,
		description: 'Media type 1 description',
		alias: 'mediaType1',
		icon: 'icon-bug',
		properties: [],
		containers: [],
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		allowedContentTypes: [],
		compositions: [],
		isFolder: false,
		hasChildren: false,
		isContainer: false,
	},
];
