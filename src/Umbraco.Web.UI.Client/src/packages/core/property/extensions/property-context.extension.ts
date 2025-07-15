import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyContext<MetaType extends MetaPropertyContext = MetaPropertyContext>
	extends ManifestWithDynamicConditions<UmbExtensionConditionConfig>,
		ManifestApi<UmbApi> {
	type: 'propertyContext';
	forPropertyEditorUis: string[];
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyContext {}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyContext: ManifestPropertyContext;
	}
}
