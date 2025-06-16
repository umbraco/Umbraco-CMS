import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDashboardApp
	extends ManifestElement,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'dashboardApp';
}

declare global {
	interface UmbExtensionManifestMap {
		umbDashboardApp: ManifestDashboardApp;
	}
}
