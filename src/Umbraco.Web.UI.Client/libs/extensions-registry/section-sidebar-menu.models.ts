import type { ManifestElement } from './models';

export interface ManifestMenuSectionSidebarApp extends ManifestElement {
	type: 'menuSectionSidebarApp';
	meta: MetaMenuSectionSidebarApp;
}

export interface MetaMenuSectionSidebarApp {
	label: string;
	sections: Array<string>;
	menu: string;
}
