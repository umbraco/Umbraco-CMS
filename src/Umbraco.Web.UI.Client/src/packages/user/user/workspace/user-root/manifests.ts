import { UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.UserRoot',
		name: 'User Root Workspace View',
		element: () => import('./user-root-workspace.element.js'),
		meta: {
			entityType: UMB_USER_ROOT_ENTITY_TYPE,
		},
	},
];
