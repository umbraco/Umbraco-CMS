import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSectionContext<MetaType extends MetaSectionContext = MetaSectionContext>
	extends ManifestWithDynamicConditions<UmbExtensionConditionConfig>,
		ManifestApi<UmbApi> {
	type: 'sectionContext';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaSectionContext {}

declare global {
	interface UmbExtensionManifestMap {
		UmbManifestSectionContext: ManifestSectionContext;
	}
}
