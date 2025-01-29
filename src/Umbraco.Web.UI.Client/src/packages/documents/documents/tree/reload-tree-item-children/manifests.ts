import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS } from '@umbraco-cms/backoffice/entity-action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.Document.Tree.ReloadChildrenOf',
		name: 'Reload Document Tree Item Children Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS,
			},
		],
	},
];
