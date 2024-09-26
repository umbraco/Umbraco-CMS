import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS,
		name: 'Script Folder Workspace',
		api: () => import('./script-folder-workspace.context.js'),
		meta: {
			entityType: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
		},
	},
];
