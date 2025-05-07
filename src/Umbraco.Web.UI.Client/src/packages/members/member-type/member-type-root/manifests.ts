import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.MemberType.Root',
		name: 'Member Type Root Workspace',
		meta: {
			entityType: UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_memberTypes',
		},
	},
];
