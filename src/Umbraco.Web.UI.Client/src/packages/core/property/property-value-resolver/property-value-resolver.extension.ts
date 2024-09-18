import type { UmbPropertyValueResolver } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValueResolver extends ManifestApi<UmbPropertyValueResolver<any>> {
	type: 'propertyValueResolver';
	meta: MetaPropertyValueResolver;
}

export interface MetaPropertyValueResolver {
	editorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueResolver: ManifestPropertyValueResolver;
	}
}
