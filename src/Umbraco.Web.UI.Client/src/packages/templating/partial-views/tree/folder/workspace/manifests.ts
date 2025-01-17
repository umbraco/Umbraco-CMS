import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS,
		name: 'Partial View Folder Workspace',
		api: () => import('./partial-view-folder-workspace.context.js'),
		meta: {
			entityType: UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
		},
	},
];
