import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItemCard extends ManifestElement {
	type: 'treeItemCard';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeItemCard: ManifestTreeItemCard;
	}
}
