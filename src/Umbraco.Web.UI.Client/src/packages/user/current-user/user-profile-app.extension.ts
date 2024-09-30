import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestUserProfileApp extends ManifestElement {
	type: 'userProfileApp';
	meta: MetaUserProfileApp;
}

export interface MetaUserProfileApp {
	label: string;
	pathname: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbUserProfileApp: ManifestUserProfileApp;
	}
}
