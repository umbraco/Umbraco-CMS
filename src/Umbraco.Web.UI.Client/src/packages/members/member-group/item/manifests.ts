import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.MemberGroup',
		name: 'Member Group Entity Item Reference',
		element: () => import('./member-group-item-ref.element.js'),
		forEntityTypes: [UMB_MEMBER_GROUP_ENTITY_TYPE],
	},
];
