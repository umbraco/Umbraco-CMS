import { UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.RelationType.Tree.ReloadChildrenOf',
		name: 'Reload Relation Type Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [UMB_RELATION_TYPE_ROOT_ENTITY_TYPE],
	},
];
