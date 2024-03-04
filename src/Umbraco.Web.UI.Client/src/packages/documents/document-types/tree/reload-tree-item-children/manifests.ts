import {
	UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
} from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Tree.ReloadChildrenOf',
		name: 'Reload Document Type Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
	},
];
