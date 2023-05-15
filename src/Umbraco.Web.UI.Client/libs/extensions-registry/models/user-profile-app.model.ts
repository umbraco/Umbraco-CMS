import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestUserProfileApp extends ManifestElement {
	type: 'userProfileApp';
	meta: MetaUserProfileApp;
}

export interface MetaUserProfileApp {
	label: string;
	pathname: string;
}
