import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.Member',
		name: 'Member Entity Item Reference',
		element: () => import('./member-item-ref.element.js'),
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
	},
	...repositoryManifests,
];
