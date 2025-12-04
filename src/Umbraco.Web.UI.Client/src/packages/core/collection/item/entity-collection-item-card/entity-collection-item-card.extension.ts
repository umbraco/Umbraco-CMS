import type { ManifestEntityCollectionItemBase } from '../entity-collection-item.extension.js';

export interface ManifestEntityCollectionItemCard<
	MetaType extends MetaEntityCollectionItemCard = MetaEntityCollectionItemCard,
> extends ManifestEntityCollectionItemBase<MetaType> {
	type: 'entityCollectionItemCard';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityCollectionItemCard {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityCollectionItemCard: ManifestEntityCollectionItemCard;
	}
}
