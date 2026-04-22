import { UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TYPE_ROOT_WORKSPACE_ALIAS } from './index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_MEDIA_TYPE_ROOT_WORKSPACE_ALIAS,
		name: 'Media Type Root Workspace',
		meta: {
			entityType: UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_mediaTypes',
		},
	},
];
