import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_TREE_ALIAS, UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export { UMB_DOCUMENT_TREE_ALIAS, UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
		name: 'Document Tree Repository',
		api: () => import('./document-tree.repository.js'),
	},
	{
		type: 'tree',
		alias: UMB_DOCUMENT_TREE_ALIAS,
		name: 'Document Tree',
		api: () => import('./document-tree.context.js'),
		element: () => import('./document-tree.element.js'),
		meta: {
			repositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		alias: 'Umb.TreeItem.Document',
		name: 'Document Tree Item',
		element: () => import('./tree-item/document-tree-item.element.js'),
		api: () => import('./tree-item/document-tree-item.context.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Document.Root',
		name: 'Document Tree Root',
		forEntityTypes: [UMB_DOCUMENT_ROOT_ENTITY_TYPE],
	},
	{
		type: 'treeItemCard',
		kind: 'default',
		alias: 'Umb.TreeItemCard.Document',
		name: 'Document Tree Item Card',
		element: () => import('./tree-item/document-tree-item-card.element.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
	...viewManifests,
	...reloadTreeItemChildrenManifests,
];
