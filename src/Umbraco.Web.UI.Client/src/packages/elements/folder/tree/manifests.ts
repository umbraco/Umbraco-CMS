import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTreeItem } from '@umbraco-cms/backoffice/tree';

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.Element.Folder',
	name: 'Element Folder Tree Item',
	api: () => import('./element-folder-tree-item.context.js'),
	forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
};

export const manifests: Array<UmbExtensionManifest> = [treeItem];
