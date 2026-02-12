import type { UmbPropertyValueResolver } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValueResolver extends ManifestApi<UmbPropertyValueResolver<any, any, any>> {
	type: 'propertyValueResolver';
	meta?: MetaPropertyValueResolver;
	forEditorAlias: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyValueResolver {}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueResolver: ManifestPropertyValueResolver;
	}
}
