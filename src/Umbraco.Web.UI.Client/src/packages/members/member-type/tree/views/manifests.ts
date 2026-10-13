import { UMB_MEMBER_TYPE_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.MemberType.Classic',
		name: 'Member Type Classic Tree View',
		forTrees: [UMB_MEMBER_TYPE_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.MemberType.Table',
		name: 'Member Type Table Tree View',
		forTrees: [UMB_MEMBER_TYPE_TREE_ALIAS],
	},
];
