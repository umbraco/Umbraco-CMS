import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceContext
	extends ManifestWithDynamicConditions<UmbExtensionCondition>,
		ManifestApi<UmbApi> {
	type: 'workspaceContext';
}
