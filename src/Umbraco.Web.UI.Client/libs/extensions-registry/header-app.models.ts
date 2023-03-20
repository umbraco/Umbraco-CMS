import type { ManifestElement } from './models';

export interface ManifestHeaderApp extends ManifestElement {
	type: 'headerApp';
	//meta: MetaHeaderApp;
	meta?: unknown;
}

export interface MetaHeaderApp {
	pathname: string;
	label: string;
	icon: string;
}

export interface ManifestHeaderAppButtonKind extends Omit<ManifestHeaderApp, 'kind' | 'meta'> {
	type: 'headerApp';
	kind: 'button';
	meta: MetaHeaderAppButtonKind;
}

export interface MetaHeaderAppButtonKind {
	href: string;
	label: string;
	icon: string;
}
