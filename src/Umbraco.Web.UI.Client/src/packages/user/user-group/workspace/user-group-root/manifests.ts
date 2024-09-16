import { UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.UserGroupRoot',
		name: 'User Group Root Workspace View',
		element: () => import('./user-group-root-workspace.element.js'),
		meta: {
			entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE,
		},
	},
];
