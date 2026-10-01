import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityPublishAwareness
	extends ManifestApi<UmbEntityPublishAwarenessApi> {
	type: 'entityPublishAwareness';
	forEntityTypes: Array<string>;
	meta: MetaEntityPublishAwareness;
}

export interface MetaEntityPublishAwareness {
	/** The item repository alias used to batch-load items of this entity type by unique. */
	itemRepositoryAlias: string;
}

/**
 * Declares how an entity type participates in publish awareness: given one of its item models (as loaded via
 * {@link MetaEntityPublishAwareness.itemRepositoryAlias}), whether it currently needs attention before the
 * thing referencing it is published — e.g. because it isn't fully published itself.
 */
export interface UmbEntityPublishAwarenessApi<ItemType = any> extends UmbApi {
	needsAttention(item: ItemType): boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestEntityPublishAwareness: ManifestEntityPublishAwareness;
	}
}
