import { UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.MediaType.Root',
		name: 'Media Type Root Workspace',
		meta: {
			entityType: UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_mediaTypes',
		},
	},
];
