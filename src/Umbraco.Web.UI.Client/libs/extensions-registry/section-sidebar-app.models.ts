import type { ManifestElement } from './models';

export interface ManifestSectionSidebarApp extends ManifestElement {
	type: 'sectionSidebarApp';
	meta: MetaSectionSidebarApp;
}

export interface MetaSectionSidebarApp {
	sections: Array<string>;
}

// TODO: this is a temp solution until we implement kinds
export interface ManifestMenuSectionSidebarApp extends ManifestElement {
	type: 'menuSectionSidebarApp';
	meta: MetaMenuSectionSidebarApp;
}

export interface MetaMenuSectionSidebarApp extends MetaSectionSidebarApp {
	label: string;
	menu: string;
}
