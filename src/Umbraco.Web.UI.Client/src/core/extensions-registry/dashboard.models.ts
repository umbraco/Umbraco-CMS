import type { ManifestElement } from './models';

export interface ManifestDashboard extends ManifestElement {
	type: 'dashboard';
	meta: MetaDashboard;
}

export interface MetaDashboard {
	sections: string[];
	pathname: string;
	label?: string;
}
