import { UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS,
		name: 'Document Blueprint Root Workspace',
		meta: {
			entityType: UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_contentBlueprints',
		},
	},
];
