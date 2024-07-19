import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.DocumentRecycleBin.Tree.ReloadChildrenOf',
		name: 'Reload Document Recycle Bin Tree Item Children Entity Action',
		forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
	},
];
