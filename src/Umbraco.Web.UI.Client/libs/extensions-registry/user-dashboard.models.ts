import type { ManifestElement } from './models';

export interface ManifestUserDashboard extends ManifestElement {
	type: 'userDashboard';
	meta: MetaUserDashboard;
}

export interface MetaUserDashboard {
	label: string;
	pathname: string;
}
