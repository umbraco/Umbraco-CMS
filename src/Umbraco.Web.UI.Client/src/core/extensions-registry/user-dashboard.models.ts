import type { ManifestElement } from './models';

export interface ManifestUserDashboard extends ManifestElement {
	type: 'user-dashboard';
	meta: MetaUserDashboard;
}

export interface MetaUserDashboard {
	label: string;
	pathname: string;
}
