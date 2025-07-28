import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export type UmbDashboardAppSize = 'small' | 'medium' | 'large';

export interface UmbDashboardAppElement extends HTMLElement {
	manifest?: ManifestDashboardApp;
	size?: UmbDashboardAppSize;
}

export interface ManifestDashboardApp
	extends ManifestElement<UmbDashboardAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'dashboardApp';
	meta: MetaDashboardApp;
}

export interface MetaDashboardApp {
	headline: string;
	size: UmbDashboardAppSize;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDashboardApp: ManifestDashboardApp;
	}
}
