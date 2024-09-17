import { UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MemberGroupRoot',
		name: 'Member Group Root Workspace View',
		element: () => import('./member-group-root-workspace.element.js'),
		meta: {
			entityType: UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE,
		},
	},
];
