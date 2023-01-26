import type { ManifestElement } from './models';

export interface ManifestSidebarMenuItem extends ManifestElement {
	type: 'sidebarMenuItem';
	meta: MetaSidebarMenuItem;
}

export interface MetaSidebarMenuItem {
	label: string;
	icon: string;
	sections: Array<string>;
	entityType?: string;
}
