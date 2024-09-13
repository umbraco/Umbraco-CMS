import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceContext
	extends ManifestWithDynamicConditions<UmbExtensionManifest>,
		ManifestApi<UmbApi> {
	type: 'workspaceContext';
}
