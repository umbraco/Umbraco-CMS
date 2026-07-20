import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeView
	extends ManifestElement,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'treeView';
	/**
	 * The tree aliases this view applies to. When omitted, the view applies to all trees.
	 */
	forTrees?: Array<string>;
	meta: MetaTreeView;
}

export interface MetaTreeView {
	/**
	 * The friendly name of the tree view
	 */
	label: string;

	/**
	 * An icon to represent the tree view
	 * @examples [
	 *   "icon-list",
	 *   "icon-grid"
	 * ]
	 */
	icon: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeView: ManifestTreeView;
	}
}
