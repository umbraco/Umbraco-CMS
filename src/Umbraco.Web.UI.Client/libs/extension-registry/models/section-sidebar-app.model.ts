import type { UmbSectionSidebarAppExtensionElement } from '../interfaces/section-sidebar-app-extension-element.interface';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSectionSidebarApp extends ManifestElement<UmbSectionSidebarAppExtensionElement> {
	type: 'sectionSidebarApp';
	conditions: ConditionsSectionSidebarApp;
}

export interface ConditionsSectionSidebarApp {
	sections: Array<string>;
}

export interface ManifestSectionSidebarAppBaseMenu extends ManifestSectionSidebarApp {
	type: 'sectionSidebarApp';
	meta: MetaSectionSidebarAppMenuKind;
}

export interface ManifestSectionSidebarAppMenuKind extends ManifestSectionSidebarAppBaseMenu {
	type: 'sectionSidebarApp';
	kind: 'menu';
}

export interface MetaSectionSidebarAppMenuKind {
	label: string;
	menu: string;
}

export interface ManifestSectionSidebarAppMenuWithEntityActionsKind extends ManifestSectionSidebarAppBaseMenu {
	type: 'sectionSidebarApp';
	kind: 'menuWithEntityActions';
	meta: MetaSectionSidebarAppMenuWithEntityActionsKind;
}

export interface MetaSectionSidebarAppMenuWithEntityActionsKind extends MetaSectionSidebarAppMenuKind {
	entityType: string;
}
