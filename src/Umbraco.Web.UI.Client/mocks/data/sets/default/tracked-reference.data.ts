import type { UmbMockTrackedReferenceItemModel } from '../../mock-data-set.types.js';
import type {
	IReferenceResponseModelDefaultReferenceResponseModel,
	IReferenceResponseModelDocumentReferenceResponseModel,
	IReferenceResponseModelMediaReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const items: Array<UmbMockTrackedReferenceItemModel> = [
	{
		$type: 'DocumentReferenceResponseModel',
		id: 'simple-document-id',
		name: 'Simple Document',
		published: true,
		documentType: {
			alias: 'blogPost',
			icon: 'icon-document',
			name: 'Simple Document Type',
			id: 'simple-document-type-id',
		},
		variants: [],
	} satisfies IReferenceResponseModelDocumentReferenceResponseModel,
	{
		$type: 'DocumentReferenceResponseModel',
		id: '1234',
		name: 'Image Block',
		published: true,
		documentType: {
			alias: 'imageBlock',
			icon: 'icon-settings',
			name: 'Image Block',
			id: 'image-block-id',
		},
		variants: [],
	} satisfies IReferenceResponseModelDocumentReferenceResponseModel,
	{
		$type: 'MediaReferenceResponseModel',
		id: 'media-id',
		name: 'Simple Media',
		mediaType: {
			alias: 'image',
			icon: 'icon-picture',
			name: 'Image',
			id: 'media-type-id',
		},
	} satisfies IReferenceResponseModelMediaReferenceResponseModel,
	{
		$type: 'DefaultReferenceResponseModel',
		id: 'default-id',
		name: 'Some other reference',
		type: 'Default',
		icon: 'icon-bug',
	} satisfies IReferenceResponseModelDefaultReferenceResponseModel,
];
