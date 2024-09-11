import { UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
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
