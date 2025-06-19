import { UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.DocumentType.Root',
		name: 'Document Type Root Workspace',
		meta: {
			entityType: UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_documentTypes',
		},
	},
];
