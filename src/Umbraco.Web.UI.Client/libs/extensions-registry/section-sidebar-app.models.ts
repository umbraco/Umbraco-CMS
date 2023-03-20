import type { ManifestElement } from './models';

export interface ManifestSectionSidebarApp extends ManifestElement {
	type: 'sectionSidebarApp';
	conditions: ConditionsSectionSidebarApp;
}

export interface ConditionsSectionSidebarApp {
	sections: Array<string>;
}

// TODO: this is a temp solution until we implement kinds
export interface ManifestMenuSectionSidebarApp extends Omit<ManifestSectionSidebarApp, 'type'> {
	type: 'menuSectionSidebarApp';
	meta: MetaMenuSectionSidebarApp;
	conditions: ConditionsSectionSidebarApp;
}

export interface MetaMenuSectionSidebarApp {
	label: string;
	menu: string;
}
