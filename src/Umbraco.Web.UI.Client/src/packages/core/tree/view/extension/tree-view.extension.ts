import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeView extends ManifestElement, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'treeView';
	meta: MetaTreeView;
}

export interface MetaTreeView {
	label: string;
	icon: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbTreeView: ManifestTreeView;
	}
}
