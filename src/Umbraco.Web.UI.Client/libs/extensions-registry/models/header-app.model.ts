import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

/**
 * Header apps are displayed in the top right corner of the backoffice
 * The two provided header apps are the search and the user menu
 */
export interface ManifestHeaderApp extends ManifestElement {
	type: 'headerApp';
	//meta: MetaHeaderApp;
}

// TODO: Warren these don't seem to be used anywhere
export interface MetaHeaderApp {
	pathname: string;
	label: string;
	icon: string;
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
