import { UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.MemberGroup.Create',
		name: 'Create Member Group Entity Action',
		weight: 1200,
		forEntityTypes: [UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE],
	},
	...defaultManifests,
];
