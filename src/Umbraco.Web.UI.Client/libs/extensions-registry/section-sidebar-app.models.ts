import type { ManifestElement } from './models';

export interface ManifestSectionSidebarApp extends ManifestElement {
	type: 'sectionSidebarApp';
	conditions: ConditionsSectionSidebarApp;
	meta?: unknown;
}

export interface ConditionsSectionSidebarApp {
	sections: Array<string>;
}

export interface ManifestSectionSidebarAppMenuKind extends Omit<ManifestSectionSidebarApp, 'kind' | 'meta'> {
	type: 'sectionSidebarApp';
	kind: 'menu';
	meta: MetaSectionSidebarAppMenuKind;
}

export interface MetaSectionSidebarAppMenuKind {
	label: string;
	menu: string;
}
