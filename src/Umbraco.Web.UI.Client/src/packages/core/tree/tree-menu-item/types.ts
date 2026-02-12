import type { ManifestMenuItem, MetaMenuItem } from '@umbraco-cms/backoffice/menu';

export interface ManifestMenuItemTreeKind extends ManifestMenuItem {
	type: 'menuItem';
	kind: 'tree';
	meta: MetaMenuItemTreeKind;
}

export interface MetaMenuItemTreeKind extends MetaMenuItem {
	treeAlias: string;
	hideTreeRoot?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeMenuItemKind: ManifestMenuItemTreeKind;
	}
}
