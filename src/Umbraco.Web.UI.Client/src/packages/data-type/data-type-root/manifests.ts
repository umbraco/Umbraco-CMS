import { UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.DataType.Root',
		name: 'Data Type Root Workspace',
		meta: {
			entityType: UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_dataTypes',
		},
	},
];
