import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.Media',
		name: 'Member Entity Item Reference',
		element: () => import('./media-item-ref.element.js'),
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	},
];
