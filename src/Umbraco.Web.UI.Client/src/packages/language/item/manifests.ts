import { UMB_LANGUAGE_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.Language',
		name: 'Language Entity Item Reference',
		element: () => import('./langauge-item-ref.element.js'),
		forEntityTypes: [UMB_LANGUAGE_ENTITY_TYPE],
	},
];
