import type {
	DefaultReferenceResponseModel,
	DocumentReferenceResponseModel,
	MediaReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const items: Array<
	DefaultReferenceResponseModel | DocumentReferenceResponseModel | MediaReferenceResponseModel
> = [
	{
		id: 'simple-document-id',
		name: 'Simple Document',
		published: true,
		documentType: {
			alias: 'blogPost',
			icon: 'icon-document',
			name: 'Simple Document Type',
		},
	} satisfies DocumentReferenceResponseModel,
	{
		id: '1234',
		name: 'Image Block',
		published: true,
		documentType: {
			alias: 'imageBlock',
			icon: 'icon-settings',
			name: 'Image Block',
		},
	} satisfies DocumentReferenceResponseModel,
	{
		id: 'media-id',
		name: 'Simple Media',
		mediaType: {
			alias: 'image',
			icon: 'icon-picture',
			name: 'Image',
		},
	} satisfies MediaReferenceResponseModel,
	{
		id: 'default-id',
		name: 'Some other reference',
		type: 'Default',
		icon: 'icon-bug',
	} satisfies DefaultReferenceResponseModel,
];
