import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestGlobalContext extends ManifestApi {
	type: 'globalContext';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbGlobalContextExtension: ManifestGlobalContext;
	}
}
