import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbTreeItemCardApi, UmbTreeItemCardElement } from './types.js';

export interface ManifestTreeItemCard extends ManifestElementAndApi<UmbTreeItemCardElement, UmbTreeItemCardApi> {
	type: 'treeItemCard';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeItemCard: ManifestTreeItemCard;
	}
}
