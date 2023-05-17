import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestUserProfileApp extends ManifestElement {
	type: 'userProfileApp';
	meta: MetaUserProfileApp;
}

export interface MetaUserProfileApp {
	label: string;
	pathname: string;
}
