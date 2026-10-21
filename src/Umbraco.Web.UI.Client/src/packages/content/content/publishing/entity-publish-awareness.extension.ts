import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Opts entity types in to publish awareness, so entities of those types that are referenced by content being
 * published can be reported when they need attention.
 */
export interface ManifestEntityPublishAwareness extends ManifestApi<UmbEntityPublishAwarenessApi> {
	type: 'entityPublishAwareness';
	/** The entity types this extension decides publish awareness for. */
	forEntityTypes: Array<string>;
	meta: MetaEntityPublishAwareness;
}

/**
 * Meta for {@link ManifestEntityPublishAwareness}.
 */
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
	/**
	 * Whether the given item needs attention before content referencing it is published.
	 * @param {ItemType} item - The item to check, as loaded from the item repository.
	 * @returns {boolean} `true` when the item needs attention.
	 */
	needsAttention(item: ItemType): boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestEntityPublishAwareness: ManifestEntityPublishAwareness;
	}
}
