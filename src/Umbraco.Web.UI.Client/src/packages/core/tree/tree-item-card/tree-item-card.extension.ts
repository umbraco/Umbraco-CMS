import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeItemCard extends ManifestElementAndApi {
	type: 'treeItemCard';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeItemCard: ManifestTreeItemCard;
	}
}
