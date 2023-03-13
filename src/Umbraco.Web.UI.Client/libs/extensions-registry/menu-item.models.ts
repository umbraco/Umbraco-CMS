import type { ManifestElement } from './models';

export interface ManifestMenuItem extends ManifestElement {
	type: 'menuItem';
	meta: MetaMenuItem;
	conditions: ConditionsMenuItem;
}

export interface MetaMenuItem {
	label: string;
	icon: string;
	entityType?: string;
}

export interface ConditionsMenuItem {
	menus: Array<string>;
}
