import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Tree.ReloadChildrenOf',
		name: 'Reload Document Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE],
	},
];
