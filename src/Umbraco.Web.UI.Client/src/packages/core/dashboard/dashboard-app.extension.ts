import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDashboardApp
	extends ManifestElement,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'dashboardApp';
	meta: MetaDashboardApp;
}

export interface MetaDashboardApp {
	headline: string;
	size: 'small' | 'medium' | 'large';
}

declare global {
	interface UmbExtensionManifestMap {
		umbDashboardApp: ManifestDashboardApp;
	}
}
