import type { ManifestDashboard, MetaDashboard } from '../dashboard.extension.js';

export interface ManifestDashboardDefaultKind extends ManifestDashboard {
	type: 'dashboard';
	kind: 'default';
}

export interface MetaDashboardDefaultKind extends MetaDashboard {
	headline: string;
}

export interface DashboardAppInstance {
	key : string;
	/** Dashboard App Alias */
	alias : string;
	rows? : number;
	columns? : number;
	headline? : string;
	component? : HTMLElement;
}

/**
 * Defines a configured dashboard app added by a user. Used e.g for serialization.
 */
export type ConfiguredDashboardApp = {
	/** Dashboard App Alias */
	alias : string;
}

export interface UserDashboardAppConfiguration {
	apps : ConfiguredDashboardApp[];
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestDashboardDefaultKind: ManifestDashboardDefaultKind;
	}
}
