import type { ManifestElement } from './models';

export interface ManifestSectionSidebarApp extends ManifestElement {
	type: 'sectionSidebarApp';
	conditions: ConditionsSectionSidebarApp;
}

export interface ConditionsSectionSidebarApp {
	sections: Array<string>;
}

export interface ManifestSectionSidebarAppMenuKind extends ManifestSectionSidebarApp {
	type: 'sectionSidebarApp';
	kind: 'menu';
	meta: MetaSectionSidebarAppMenuKind;
}

export interface MetaSectionSidebarAppMenuKind {
	label: string;
	menu: string;
}
