import { UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Script.Tree.ReloadChildrenOf',
		name: 'Reload Script Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
	},
];
