import type { UmbPropertyValueResolver } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValueResolver extends ManifestApi<UmbPropertyValueResolver<any, any, any>> {
	type: 'propertyValueResolver';
	meta?: MetaPropertyValueResolver;
	forEditorAlias: string;
}

export interface MetaPropertyValueResolver {
	/**
	 * @deprecated use `forEditorAlias` instead, will be removed in v.17
	 */
	editorAlias?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueResolver: ManifestPropertyValueResolver;
	}
}
