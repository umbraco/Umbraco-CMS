import type { UmbSectionSidebarAppExtensionElement } from '../interfaces/section-sidebar-app-extension-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSectionSidebarApp
	extends ManifestElement<UmbSectionSidebarAppExtensionElement>,
		ManifestWithDynamicConditions {
	type: 'sectionSidebarApp';
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
