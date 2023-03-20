import type { ManifestElement, ManifestWithConditions } from './models';

export interface ManifestDashboard extends ManifestElement, ManifestWithConditions<ConditionsDashboard> {
	type: 'dashboard';
	meta: MetaDashboard;
}

export interface MetaDashboard {
	pathname: string;
	label?: string;
}

export interface ConditionsDashboard {
	sections: string[];
}
