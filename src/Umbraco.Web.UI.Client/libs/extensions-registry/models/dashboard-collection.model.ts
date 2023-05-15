import type { ManifestBase } from '@umbraco-cms/backoffice/extensions-api';

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
