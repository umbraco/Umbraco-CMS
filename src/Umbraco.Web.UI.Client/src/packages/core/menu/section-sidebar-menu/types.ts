import type { ManifestSectionSidebarApp } from '@umbraco-cms/backoffice/section';
import type { UmbEntityExpansionEntryModel } from '@umbraco-cms/backoffice/utils';

export interface MetaSectionSidebarAppMenuKind {
	label: string;
	menu: string;
}

export interface ManifestSectionSidebarAppBaseMenu extends ManifestSectionSidebarApp {
	type: 'sectionSidebarApp';
	meta: MetaSectionSidebarAppMenuKind;
}

export interface ManifestSectionSidebarAppMenuKind extends ManifestSectionSidebarAppBaseMenu {
	type: 'sectionSidebarApp';
	kind: 'menu';
}

export interface ManifestSectionSidebarAppMenuWithEntityActionsKind extends ManifestSectionSidebarAppBaseMenu {
	type: 'sectionSidebarApp';
	kind: 'menuWithEntityActions';
	meta: MetaSectionSidebarAppMenuWithEntityActionsKind;
}

export interface MetaSectionSidebarAppMenuWithEntityActionsKind extends MetaSectionSidebarAppMenuKind {
	entityType: string;
}

export interface UmbEntityExpansionSectionEntryModel extends UmbEntityExpansionEntryModel {
	sectionAlias?: string;
}
