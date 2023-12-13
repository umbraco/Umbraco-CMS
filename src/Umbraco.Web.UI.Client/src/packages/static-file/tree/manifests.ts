import { UMB_STATIC_FILE_ENTITY_TYPE, UMB_STATIC_FILE_ROOT_ENTITY_TYPE } from '../entity.js';
import type { ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'fileSystem',
	alias: 'Umb.TreeItem.StaticFile',
	name: 'Static File Tree Item',
	meta: {
		entityTypes: [UMB_STATIC_FILE_ENTITY_TYPE, UMB_STATIC_FILE_ROOT_ENTITY_TYPE],
	},
};

export const manifests = [treeItem];
