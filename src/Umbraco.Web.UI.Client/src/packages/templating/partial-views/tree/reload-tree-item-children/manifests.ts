import {
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
} from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Tree.ReloadChildrenOf',
		name: 'Reload Partial View Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [
			UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			UMB_PARTIAL_VIEW_ENTITY_TYPE,
			UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
		],
	},
];
