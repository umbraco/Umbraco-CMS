import type { UmbEntityCollectionItemElement } from '../entity-collection-item-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityCollectionItemRef<
	MetaType extends MetaEntityCollectionItemRef = MetaEntityCollectionItemRef,
> extends ManifestElement<UmbEntityCollectionItemElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entityCollectionItemRef';
	meta: MetaType;
	forEntityTypes: Array<string>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityCollectionItemRef {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityCollectionItemRef: ManifestEntityCollectionItemRef;
	}
}
