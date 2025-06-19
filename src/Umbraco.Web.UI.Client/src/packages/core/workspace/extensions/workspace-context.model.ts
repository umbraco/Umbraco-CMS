import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceContext
	extends ManifestWithDynamicConditions<UmbExtensionConditionConfig>,
		ManifestApi<UmbApi> {
	type: 'workspaceContext';
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestWorkspaceContext: ManifestWorkspaceContext;
	}
}
