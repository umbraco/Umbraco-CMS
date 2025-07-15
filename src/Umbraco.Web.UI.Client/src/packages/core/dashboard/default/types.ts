import type { ManifestDashboard, MetaDashboard } from '../dashboard.extension.js';

export interface ManifestDashboardDefaultKind extends ManifestDashboard {
	type: 'dashboard';
	kind: 'default';
}

export interface MetaDashboardDefaultKind extends MetaDashboard {
	headline: string;
}

export interface DashboardAppInstance {
	key? : string;
	rows? : number;
	columns? : number;
	headline? : string;
	component? : HTMLElement;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestDashboardDefaultKind: ManifestDashboardDefaultKind;
	}
}
