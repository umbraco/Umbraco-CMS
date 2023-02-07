import type { ManifestCollectionBulkAction } from './collection-bulk-action.models';
import type { ManifestCollectionView } from './collection-view.models';
import type { ManifestDashboard } from './dashboard.models';
import type { ManifestDashboardCollection } from './dashboard-collection.models';
import type { ManifestEntityAction } from './entity-action.models';
import type { ManifestEntityBulkAction } from './entity-bulk-action.models';
import type { ManifestExternalLoginProvider } from './external-login-provider.models';
import type { ManifestHeaderApp } from './header-app.models';
import type { ManifestHealthCheck } from './health-check.models';
import type { ManifestPackageView } from './package-view.models';
import type { ManifestPropertyAction } from './property-action.models';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.models';
import type { ManifestSection } from './section.models';
import type { ManifestSectionView } from './section-view.models';
import type { ManifestSidebarMenuItem } from './sidebar-menu-item.models';
import type { ManifestTheme } from './theme.models';
import type { ManifestTree } from './tree.models';
import type { ManifestTreeItemAction } from './tree-item-action.models';
import type { ManifestUserDashboard } from './user-dashboard.models';
import type { ManifestWorkspace } from './workspace.models';
import type { ManifestWorkspaceAction } from './workspace-action.models';
import type { ManifestWorkspaceView } from './workspace-view.models';
import type { ManifestWorkspaceViewCollection } from './workspace-view-collection.models';
import type { ManifestRepository } from './repository.models';
import type { ClassConstructor } from '@umbraco-cms/models';

export * from './collection-bulk-action.models';
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
export * from './sidebar-menu-item.models';
export * from './theme.models';
export * from './tree-item-action.models';
export * from './tree.models';
export * from './user-dashboard.models';
export * from './workspace-action.models';
export * from './workspace-view-collection.models';
export * from './workspace-view.models';
export * from './repository.models';
export * from './workspace.models';

export type ManifestTypes =
	| ManifestCollectionBulkAction
	| ManifestCollectionView
	| ManifestCustom
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestEntityAction
	| ManifestEntityBulkAction
	| ManifestEntrypoint
	| ManifestExternalLoginProvider
	| ManifestHeaderApp
	| ManifestHealthCheck
	| ManifestPackageView
	| ManifestPropertyAction
	| ManifestPropertyEditorModel
	| ManifestPropertyEditorUI
	| ManifestRepository
	| ManifestSection
	| ManifestSectionView
	| ManifestSidebarMenuItem
	| ManifestTheme
	| ManifestTree
	| ManifestTreeItemAction
	| ManifestUserDashboard
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceView
	| ManifestWorkspaceViewCollection;

export type ManifestStandardTypes = ManifestTypes['type'];

export type ManifestTypeMap = {
	[Manifest in ManifestTypes as Manifest['type']]: Manifest;
};

export interface ManifestBase {
	type: string;
	alias: string;
	name: string;
	weight?: number;
}

export interface ManifestWithLoader<LoaderReturnType> extends ManifestBase {
	loader?: () => Promise<LoaderReturnType>;
}

export interface ManifestClass extends ManifestWithLoader<object> {
	type: ManifestStandardTypes;
	js?: string;
	className?: string;
	class?: ClassConstructor<unknown>;
	//loader?: () => Promise<object | HTMLElement>;
}

export interface ManifestClassWithClassConstructor extends ManifestClass {
	class: ClassConstructor<unknown>;
}

export interface ManifestElement extends ManifestWithLoader<object | HTMLElement> {
	type: ManifestStandardTypes;
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

export interface ManifestCustom extends ManifestBase {
	type: 'custom';
	meta?: any;
}

export interface ManifestWithMeta extends ManifestBase {
	meta: any;
}

export interface ManifestEntrypoint extends ManifestBase {
	type: 'entrypoint';
	js: string;
}
