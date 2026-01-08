import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityItemRef<MetaType extends MetaEntityItemRef = MetaEntityItemRef>
	extends ManifestElement<any>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entityItemRef';
	meta: MetaType;
	forEntityTypes: Array<string>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityItemRef {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityItemRef: ManifestEntityItemRef;
	}
}
