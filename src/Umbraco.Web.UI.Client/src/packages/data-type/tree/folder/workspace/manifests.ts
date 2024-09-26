import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
		name: 'Data Type Folder Workspace',
		api: () => import('./data-type-folder-workspace.context.js'),
		meta: {
			entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
		},
	},
];
