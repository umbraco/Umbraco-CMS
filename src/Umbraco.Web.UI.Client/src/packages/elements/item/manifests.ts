import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.Element',
		name: 'Element Entity Item Reference',
		element: () => import('./element-item-ref.element.js'),
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
	},
	...repositoryManifests,
];
