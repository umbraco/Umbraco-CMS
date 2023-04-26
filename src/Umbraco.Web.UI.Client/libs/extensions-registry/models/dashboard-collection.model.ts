import type { ManifestBase } from './models';

export interface ManifestDashboardCollection extends ManifestBase {
	type: 'dashboardCollection';
	meta: MetaDashboardCollection;
	conditions: ConditionsDashboardCollection;
}

export interface MetaDashboardCollection {
	pathname: string;
	label?: string;
	repositoryAlias: string;
}

export interface ConditionsDashboardCollection {
	sections: string[];
	entityType: string;
}
