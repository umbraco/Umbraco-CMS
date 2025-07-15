import { UMB_USER_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.User',
		name: 'User Entity Item Reference',
		element: () => import('./user-item-ref.element.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
	},
];
