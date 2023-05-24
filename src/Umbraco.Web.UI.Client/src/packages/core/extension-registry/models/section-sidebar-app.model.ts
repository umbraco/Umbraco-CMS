import type { UmbSectionSidebarAppExtensionElement } from '../interfaces/section-sidebar-app-extension-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

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
