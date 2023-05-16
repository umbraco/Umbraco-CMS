import type { ManifestCollectionView } from './collection-view.model';
import type { ManifestDashboard } from './dashboard.model';
import type { ManifestDashboardCollection } from './dashboard-collection.model';
import type { ManifestEntityAction } from './entity-action.model';
import type { ManifestEntityBulkAction } from './entity-bulk-action.model';
import type { ManifestExternalLoginProvider } from './external-login-provider.model';
import type { ManifestHeaderApp, ManifestHeaderAppButtonKind } from './header-app.model';
import type { ManifestHealthCheck } from './health-check.model';
import type { ManifestMenu } from './menu.model';
import type { ManifestMenuItem, ManifestMenuItemTreeKind } from './menu-item.model';
import type { ManifestModal } from './modal.model';
import type { ManifestPackageView } from './package-view.model';
import type { ManifestPropertyAction } from './property-action.model';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.model';
import type { ManifestRepository } from './repository.model';
import type { ManifestSection } from './section.model';
import type { ManifestSectionSidebarApp, ManifestSectionSidebarAppMenuKind } from './section-sidebar-app.model';
import type { ManifestSectionView } from './section-view.model';
import type { ManifestStore, ManifestTreeStore, ManifestItemStore } from './store.model';
import type { ManifestTheme } from './theme.model';
import type { ManifestTree } from './tree.model';
import type { ManifestTreeItem } from './tree-item.model';
import type { ManifestUserProfileApp } from './user-profile-app.model';
import type { ManifestWorkspace } from './workspace.model';
import type { ManifestWorkspaceAction } from './workspace-action.model';
import type { ManifestWorkspaceEditorView } from './workspace-editor-view.model';
import type { ManifestWorkspaceViewCollection } from './workspace-view-collection.model';
import type { ManifestBase, ManifestEntryPoint } from '@umbraco-cms/backoffice/extensions-api';

export * from './collection-view.model';
export * from './dashboard-collection.model';
export * from './dashboard.model';
export * from './entity-action.model';
export * from './entity-bulk-action.model';
export * from './external-login-provider.model';
export * from './header-app.model';
export * from './health-check.model';
export * from './menu-item.model';
export * from './menu.model';
export * from './modal.model';
export * from './package-view.model';
export * from './property-action.model';
export * from './property-editor.model';
export * from './repository.model';
export * from './section-sidebar-app.model';
export * from './section-view.model';
export * from './section.model';
export * from './store.model';
export * from './theme.model';
export * from './tree-item.model';
export * from './tree.model';
export * from './user-profile-app.model';
export * from './workspace-action.model';
export * from './workspace-view-collection.model';
export * from './workspace-editor-view.model';
export * from './workspace.model';

export type ManifestTypes =
	| ManifestCollectionView
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestEntityAction
	| ManifestEntityBulkAction
	| ManifestEntryPoint
	| ManifestExternalLoginProvider
	| ManifestHeaderApp
	| ManifestHeaderAppButtonKind
	| ManifestHealthCheck
	| ManifestItemStore
	| ManifestMenu
	| ManifestMenuItem
	| ManifestMenuItemTreeKind
	| ManifestModal
	| ManifestPackageView
	| ManifestPropertyAction
	| ManifestPropertyEditorModel
	| ManifestPropertyEditorUI
	| ManifestRepository
	| ManifestSection
	| ManifestSectionSidebarApp
	| ManifestSectionSidebarAppMenuKind
	| ManifestSectionView
	| ManifestStore
	| ManifestTheme
	| ManifestTree
	| ManifestTreeItem
	| ManifestTreeStore
	| ManifestUserProfileApp
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceEditorView
	| ManifestWorkspaceViewCollection
	| ManifestBase;
