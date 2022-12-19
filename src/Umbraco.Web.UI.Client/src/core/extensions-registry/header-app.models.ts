import type { ManifestElement } from './models';

export interface ManifestHeaderApp extends ManifestElement {
	type: 'headerApp';
	meta: MetaHeaderApp;
}

export interface MetaHeaderApp {
	pathname: string;
	label: string;
	icon: string;
}
