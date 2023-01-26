import type { ManifestBase } from './models';

export interface ManifestDashboardCollection extends ManifestBase {
	type: 'dashboardCollection';
	meta: MetaDashboardCollection;
}

export interface MetaDashboardCollection {
	sections: string[];
	pathname: string;
	label?: string;
	entityType: string;
	storeAlias: string;
}
