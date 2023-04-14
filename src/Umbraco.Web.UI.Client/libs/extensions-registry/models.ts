import type { ManifestCollectionView } from './collection-view.models';
import type { ManifestDashboard } from './dashboard.models';
import type { ManifestDashboardCollection } from './dashboard-collection.models';
import type { ManifestEntityAction } from './entity-action.models';
import type { ManifestEntityBulkAction } from './entity-bulk-action.models';
import type { ManifestExternalLoginProvider } from './external-login-provider.models';
import type { ManifestHeaderApp, ManifestHeaderAppButtonKind } from './header-app.models';
import type { ManifestHealthCheck } from './health-check.models';
import type { ManifestPackageView } from './package-view.models';
import type { ManifestPropertyAction } from './property-action.models';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.models';
import type { ManifestSection } from './section.models';
import type { ManifestSectionView } from './section-view.models';
import type { ManifestSectionSidebarApp, ManifestSectionSidebarAppMenuKind } from './section-sidebar-app.models';
import type { ManifestMenu } from './menu.models';
import type { ManifestMenuItem, ManifestMenuItemTreeKind } from './menu-item.models';
import type { ManifestTheme } from './theme.models';
import type { ManifestTree } from './tree.models';
import type { ManifestTreeItem } from './tree-item.models';
import type { ManifestUserProfileApp } from './user-profile-app.models';
import type { ManifestWorkspace } from './workspace.models';
import type { ManifestWorkspaceAction } from './workspace-action.models';
import type { ManifestWorkspaceView } from './workspace-view.models';
import type { ManifestWorkspaceViewCollection } from './workspace-view-collection.models';
import type { ManifestRepository } from './repository.models';
import type { ManifestModal } from './modal.models';
import type { ManifestStore, ManifestTreeStore } from './store.models';
import type { ClassConstructor } from '@umbraco-cms/backoffice/models';

export * from './collection-view.models';
export * from './dashboard-collection.models';
export * from './dashboard.models';
export * from './entity-action.models';
export * from './entity-bulk-action.models';
export * from './external-login-provider.models';
export * from './header-app.models';
export * from './health-check.models';
export * from './package-view.models';
export * from './property-action.models';
export * from './property-editor.models';
export * from './section-view.models';
export * from './section.models';
export * from './section-sidebar-app.models';
export * from './menu.models';
export * from './menu-item.models';
export * from './theme.models';
export * from './tree.models';
export * from './tree-item.models';
export * from './user-profile-app.models';
export * from './workspace-action.models';
export * from './workspace-view-collection.models';
export * from './workspace-view.models';
export * from './repository.models';
export * from './store.models';
export * from './workspace.models';
export * from './modal.models';

export type ManifestTypes =
	| ManifestCollectionView
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestEntityAction
	| ManifestEntityBulkAction
	| ManifestEntrypoint
	| ManifestExternalLoginProvider
	| ManifestHeaderApp
	| ManifestHeaderAppButtonKind
	| ManifestHealthCheck
	| ManifestPackageView
	| ManifestPropertyAction
	| ManifestPropertyEditorModel
	| ManifestPropertyEditorUI
	| ManifestRepository
	| ManifestSection
	| ManifestSectionSidebarApp
	| ManifestSectionSidebarAppMenuKind
	| ManifestSectionView
	| ManifestMenu
	| ManifestMenuItem
	| ManifestMenuItemTreeKind
	| ManifestTheme
	| ManifestTree
	| ManifestTreeItem
	| ManifestUserProfileApp
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceView
	| ManifestWorkspaceViewCollection
	| ManifestModal
	| ManifestStore
	| ManifestTreeStore
	| ManifestBase;

export type ManifestStandardTypes = ManifestTypes['type'];

export type ManifestTypeMap = {
	[Manifest in ManifestTypes as Manifest['type']]: Manifest;
};

export type SpecificManifestTypeOrManifestBase<T extends keyof ManifestTypeMap | string> =
	T extends keyof ManifestTypeMap ? ManifestTypeMap[T] : ManifestBase;

export interface ManifestBase {
	type: string;
	alias: string;
	kind?: any; // I had to add the optional kind property set to undefined. To make the ManifestTypes recognize the Manifest Kind types. Notice that Kinds has to Omit the kind property when extending.
	name: string;
	weight?: number;
}

export interface ManifestKind {
	type: 'kind';
	alias: string;
	matchType: string;
	matchKind: string;
	manifest: Partial<ManifestTypes>;
}

export interface ManifestWithConditions<ConditionsType> {
	conditions: ConditionsType;
}

export interface ManifestWithLoader<LoaderReturnType> extends ManifestBase {
	loader?: () => Promise<LoaderReturnType>;
}

export interface ManifestClass<T = unknown> extends ManifestWithLoader<object> {
	//type: ManifestStandardTypes;
	js?: string;
	className?: string;
	class?: ClassConstructor<T>;
	//loader?: () => Promise<object | HTMLElement>;
}

export interface ManifestClassWithClassConstructor extends ManifestClass {
	class: ClassConstructor<unknown>;
}

export interface ManifestElement extends ManifestWithLoader<object | HTMLElement> {
	//type: ManifestStandardTypes;
	js?: string;
	elementName?: string;
	//loader?: () => Promise<object | HTMLElement>;
	meta?: any;
}

export interface ManifestWithView extends ManifestElement {
	meta: MetaManifestWithView;
}

export interface MetaManifestWithView {
	pathname: string;
	label: string;
	icon: string;
}

export interface ManifestElementWithElementName extends ManifestElement {
	elementName: string;
}

// TODO: Remove Custom as it has no purpose currently:
/*
export interface ManifestCustom extends ManifestBase {
	type: 'custom';
	meta?: unknown;
}
*/

export interface ManifestWithMeta extends ManifestBase {
	meta: unknown;
}

export interface ManifestEntrypoint extends ManifestBase {
	type: 'entrypoint';
	js: string;
}
