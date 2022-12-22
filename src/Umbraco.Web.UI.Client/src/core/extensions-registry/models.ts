import type { ManifestHeaderApp } from './header-app.models';
import type { ManifestSection } from './section.models';
import type { ManifestSectionView } from './section-view.models';
import type { ManifestTree } from './tree.models';
import type { ManifestTreeItemAction } from './tree-item-action.models';
import type { ManifestWorkspace } from './workspace.models';
import type { ManifestWorkspaceAction } from './workspace-action.models';
import type { ManifestWorkspaceView } from './workspace-view.models';
import type { ManifestWorkspaceViewCollection } from './workspace-view-collection.models';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.models';
import type { ManifestDashboard } from './dashboard.models';
import type { ManifestDashboardCollection } from './dashboard-collection.models';
import type { ManifestUserDashboard } from './user-dashboard.models';
import type { ManifestPropertyAction } from './property-action.models';
import type { ManifestPackageView } from './package-view.models';
import type { ManifestExternalLoginProvider } from './external-login-provider.models';
import type { ManifestCollectionBulkAction } from './collection-bulk-action.models';
import type { ManifestCollectionView } from './collection-view.models';

export * from './header-app.models';
export * from './section.models';
export * from './section-view.models';
export * from './tree.models';
export * from './tree-item-action.models';
export * from './workspace.models';
export * from './workspace-action.models';
export * from './workspace-view.models';
export * from './workspace-view-collection.models';
export * from './property-editor.models';
export * from './dashboard.models';
export * from './dashboard-collection.models';
export * from './user-dashboard.models';
export * from './property-action.models';
export * from './package-view.models';
export * from './external-login-provider.models';
export * from './collection-bulk-action.models';
export * from './collection-view.models';

export type ManifestTypes =
	| ManifestHeaderApp
	| ManifestSection
	| ManifestSectionView
	| ManifestTree
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceView
	| ManifestWorkspaceViewCollection
	| ManifestTreeItemAction
	| ManifestPropertyEditorUI
	| ManifestPropertyEditorModel
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestUserDashboard
	| ManifestPropertyAction
	| ManifestPackageView
	| ManifestExternalLoginProvider
	| ManifestEntrypoint
	| ManifestCustom
	| ManifestCollectionBulkAction
	| ManifestCollectionView;

export type ManifestStandardTypes =
	| 'headerApp'
	| 'section'
	| 'sectionView'
	| 'tree'
	| 'workspace'
	| 'workspaceAction'
	| 'workspaceView'
	| 'workspaceViewCollection'
	| 'treeItemAction'
	| 'propertyEditorUI'
	| 'propertyEditorModel'
	| 'dashboard'
	| 'dashboardCollection'
	| 'user-dashboard'
	| 'propertyAction'
	| 'packageView'
	| 'entrypoint'
	| 'externalLoginProvider'
	| 'collectionBulkAction'
	| 'collectionView';

export type ManifestElementType =
	| ManifestSection
	| ManifestSectionView
	| ManifestTree
	| ManifestTreeItemAction
	| ManifestWorkspace
	| ManifestWorkspaceView
	| ManifestPropertyAction
	| ManifestPropertyEditorUI
	| ManifestDashboard
	| ManifestUserDashboard
	| ManifestWorkspaceAction
	| ManifestPackageView
	| ManifestExternalLoginProvider
	| ManifestCollectionBulkAction
	| ManifestCollectionView;

export interface ManifestBase {
	type: string;
	alias: string;
	name: string;
	weight?: number;
}

export interface ManifestElement extends ManifestBase {
	type: ManifestStandardTypes;
	js?: string;
	elementName?: string;
	loader?: () => Promise<object | HTMLElement>;
	meta?: any;
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
