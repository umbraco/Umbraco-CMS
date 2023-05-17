import type { UmbSectionSidebarAppExtensionElement } from '../interfaces/section-sidebar-app-extension-element.interface';
import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestSectionSidebarApp extends ManifestElement<UmbSectionSidebarAppExtensionElement> {
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
