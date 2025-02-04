import type { ManifestTreeItem } from '@umbraco-cms/backoffice/tree';

export interface ManifestTreeItemRecycleBinKind extends ManifestTreeItem {
	type: 'treeItem';
	kind: 'recycleBin';
	meta: MetaTreeItemRecycleBinKind;
}

export interface MetaTreeItemRecycleBinKind {
	supportedEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestTreeItemRecycleBinKind: ManifestTreeItemRecycleBinKind;
	}
}
