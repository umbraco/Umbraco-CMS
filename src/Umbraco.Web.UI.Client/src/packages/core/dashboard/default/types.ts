import type { ManifestDashboard, MetaDashboard } from '../dashboard.extension.js';

export interface ManifestDashboardDefaultKind extends ManifestDashboard {
	type: 'dashboard';
	kind: 'default';
}

export interface MetaDashboardDefaultKind extends MetaDashboard {
	headline: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestDashboardDefaultKind: ManifestDashboardDefaultKind;
	}
}
