import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

/**
 * Header apps are displayed in the top right corner of the backoffice
 * The two provided header apps are the search and the user menu
 */
export interface ManifestHeaderApp extends ManifestElement {
	type: 'headerApp';
	//meta: MetaHeaderApp;
}

export interface ManifestHeaderAppButtonKind extends ManifestHeaderApp {
	type: 'headerApp';
	kind: 'button';
	meta: MetaHeaderAppButtonKind;
}

export interface MetaHeaderAppButtonKind {
	href: string;
	label: string;
	icon: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbHeaderAppExtension: ManifestHeaderApp | ManifestHeaderAppButtonKind;
	}
}
