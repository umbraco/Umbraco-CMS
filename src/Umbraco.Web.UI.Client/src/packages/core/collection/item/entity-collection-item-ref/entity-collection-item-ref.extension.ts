import type { ManifestEntityCollectionItemBase } from '../entity-collection-item.extension.js';

export interface ManifestEntityCollectionItemRef<
	MetaType extends MetaEntityCollectionItemRef = MetaEntityCollectionItemRef,
> extends ManifestEntityCollectionItemBase<MetaType> {
	type: 'entityCollectionItemRef';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityCollectionItemRef {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityCollectionItemRef: ManifestEntityCollectionItemRef;
	}
}
