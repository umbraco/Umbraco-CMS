import type { ManifestElement } from './models';

/**
 * Header apps are displayed in the top right corner of the backoffice
 * The two provided header apps are the search and the user menu
 */
export interface ManifestHeaderApp extends ManifestElement {
	type: 'headerApp';
	meta: MetaHeaderApp;
}

// TODO: Warren these don't seem to be used anywhere
export interface MetaHeaderApp {
	pathname: string;
	label: string;
	icon: string;
}
