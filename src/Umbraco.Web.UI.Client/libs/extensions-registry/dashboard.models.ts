import type { ManifestElement } from './models';

export interface ManifestDashboard extends ManifestElement {
	type: 'dashboard';
	meta: MetaDashboard;
	conditions: ConditionsDashboard;
}

export interface MetaDashboard {
	pathname: string;
	label?: string;
}

export interface ConditionsDashboard {
	sections: string[];
}
