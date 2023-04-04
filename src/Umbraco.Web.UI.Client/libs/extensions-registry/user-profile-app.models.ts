import type { ManifestElement } from './models';

export interface ManifestUserProfileApp extends ManifestElement {
	type: 'userProfileApp';
	meta: MetaUserProfileApp;
}

export interface MetaUserProfileApp {
	label: string;
	pathname: string;
}
