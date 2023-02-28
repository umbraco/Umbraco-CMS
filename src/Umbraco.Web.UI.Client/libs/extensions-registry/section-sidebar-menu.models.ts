import type { ManifestElement } from './models';

export interface ManifestSectionSidebarMenu extends ManifestElement {
	type: 'sectionSidebarMenu';
	meta: MetaSectionSidebarMenu;
}

export interface MetaSectionSidebarMenu {
	label: string;
	sections: Array<string>;
}
