import type { ManifestElement } from './models';

export interface ManifestMenuItem extends ManifestElement {
	type: 'menuItem';
	meta: MetaMenuItem;
}

export interface MetaMenuItem {
	label: string;
	icon: string;
	menus: Array<string>;
	entityType?: string;
}
