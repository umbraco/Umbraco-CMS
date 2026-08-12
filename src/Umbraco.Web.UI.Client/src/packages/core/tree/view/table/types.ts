import type { ManifestTreeView, MetaTreeView } from '../tree-view.extension.js';

export interface MetaTreeViewTableKindColumn {
	/** The property name on the tree item model to display in this column. */
	field: string;
	/** The column header label. Supports localization strings (e.g. `#general_status`). */
	label: string;
	/** Optional value type for rendering a value summary in this column. */
	valueType?: keyof UmbValueTypeMap;
}

export interface MetaTreeViewTableKind extends Partial<MetaTreeView> {
	/** Additional columns to render between the name and entity actions columns. */
	columns?: Array<MetaTreeViewTableKindColumn>;
}

export interface ManifestTreeViewTableKind extends Omit<ManifestTreeView, 'meta'> {
	type: 'treeView';
	kind: 'table';
	meta: MetaTreeViewTableKind;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeViewTableKind: ManifestTreeViewTableKind;
	}
}
