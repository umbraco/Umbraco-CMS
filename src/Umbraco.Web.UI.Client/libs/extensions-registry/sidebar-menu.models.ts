import type { ManifestElement } from './models';

export interface ManifestSidebarMenu extends ManifestElement {
	type: 'sidebarMenu';
	meta: MetaSidebarMenu;
}

export interface MetaSidebarMenu {
	label: string;
	sections: Array<string>;
}
