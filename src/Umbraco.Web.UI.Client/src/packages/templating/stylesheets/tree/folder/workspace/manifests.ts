import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_STYLESHEET_FOLDER_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_STYLESHEET_FOLDER_WORKSPACE_ALIAS,
		name: 'Stylesheet Folder Workspace',
		api: () => import('./stylesheet-folder-workspace.context.js'),
		meta: {
			entityType: UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
		},
	},
];
