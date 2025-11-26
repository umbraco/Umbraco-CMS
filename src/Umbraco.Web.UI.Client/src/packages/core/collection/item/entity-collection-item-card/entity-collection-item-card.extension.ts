import type { UmbEntityCollectionItemElement } from '../entity-collection-item-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityCollectionItemCard<
	MetaType extends MetaEntityCollectionItemCard = MetaEntityCollectionItemCard,
> extends ManifestElement<UmbEntityCollectionItemElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entityCollectionItemCard';
	meta: MetaType;
	forEntityTypes: Array<string>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityCollectionItemCard {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityCollectionItemCard: ManifestEntityCollectionItemCard;
	}
}
