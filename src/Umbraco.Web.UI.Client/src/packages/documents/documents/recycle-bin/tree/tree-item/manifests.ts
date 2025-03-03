import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'treeItem',
		kind: 'recycleBin',
		alias: 'Umb.TreeItem.Document.RecycleBin',
		name: 'Document Recycle Bin Tree Item',
		forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
		meta: {
			supportedEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
];
