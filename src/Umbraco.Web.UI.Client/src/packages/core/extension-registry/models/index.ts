import type { ManifestCollectionView } from './collection-view.model.js';
import type { ManifestDashboard } from './dashboard.model.js';
import type { ManifestDashboardCollection } from './dashboard-collection.model.js';
import type { ManifestEntityAction } from './entity-action.model.js';
import type { ManifestEntityBulkAction } from './entity-bulk-action.model.js';
import type { ManifestExternalLoginProvider } from './external-login-provider.model.js';
import type { ManifestGlobalContext } from './global-context.model.js';
import type { ManifestHeaderApp, ManifestHeaderAppButtonKind } from './header-app.model.js';
import type { ManifestHealthCheck } from './health-check.model.js';
import type { ManifestMenu } from './menu.model.js';
import type { ManifestMenuItem, ManifestMenuItemTreeKind } from './menu-item.model.js';
import type { ManifestModal } from './modal.model.js';
import type { ManifestPackageView } from './package-view.model.js';
import type { ManifestPropertyAction } from './property-action.model.js';
import type { ManifestPropertyEditorUi, ManifestPropertyEditorSchema } from './property-editor.model.js';
import type { ManifestRepository } from './repository.model.js';
import type { ManifestSection } from './section.model.js';
import type { ManifestSectionSidebarApp, ManifestSectionSidebarAppMenuKind } from './section-sidebar-app.model.js';
import type { ManifestSectionView } from './section-view.model.js';
import type { ManifestStore, ManifestTreeStore, ManifestItemStore } from './store.model.js';
import type { ManifestTheme } from './theme.model.js';
import type { ManifestTinyMcePlugin } from './tinymce-plugin.model.js';
import type { ManifestTree } from './tree.model.js';
import type { ManifestTreeItem } from './tree-item.model.js';
import type { ManifestUserProfileApp } from './user-profile-app.model.js';
import type { ManifestWorkspace } from './workspace.model.js';
import type { ManifestWorkspaceAction } from './workspace-action.model.js';
import type { ManifestWorkspaceEditorView } from './workspace-editor-view.model.js';
import type { ManifestWorkspaceViewCollection } from './workspace-view-collection.model.js';
import type {
	ManifestBase,
	ManifestBundle,
	ManifestCondition,
	ManifestEntryPoint,
} from '@umbraco-cms/backoffice/extension-api';

export * from './collection-view.model.js';
export * from './dashboard-collection.model.js';
export * from './dashboard.model.js';
export * from './entity-action.model.js';
export * from './entity-bulk-action.model.js';
export * from './external-login-provider.model.js';
export * from './global-context.model.js';
export * from './header-app.model.js';
export * from './health-check.model.js';
export * from './menu-item.model.js';
export * from './menu.model.js';
export * from './modal.model.js';
export * from './package-view.model.js';
export * from './property-action.model.js';
export * from './property-editor.model.js';
export * from './repository.model.js';
export * from './section-sidebar-app.model.js';
export * from './section-view.model.js';
export * from './section.model.js';
export * from './store.model.js';
export * from './theme.model.js';
export * from './tinymce-plugin.model.js';
export * from './tree-item.model.js';
export * from './tree.model.js';
export * from './user-profile-app.model.js';
export * from './workspace-action.model.js';
export * from './workspace-view-collection.model.js';
export * from './workspace-editor-view.model.js';
export * from './workspace.model.js';

export type ManifestTypes =
	| ManifestBundle<ManifestTypes>
	| ManifestCondition
	| ManifestCollectionView
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestEntityAction
	| ManifestEntityBulkAction
	| ManifestEntryPoint
	| ManifestExternalLoginProvider
	| ManifestGlobalContext
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
	| ManifestPropertyEditorSchema
	| ManifestPropertyEditorUi
	| ManifestRepository
	| ManifestSection
	| ManifestSectionSidebarApp
	| ManifestSectionSidebarAppMenuKind
	| ManifestSectionView
	| ManifestStore
	| ManifestTheme
	| ManifestTinyMcePlugin
	| ManifestTree
	| ManifestTreeItem
	| ManifestTreeStore
	| ManifestUserProfileApp
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceEditorView
	| ManifestWorkspaceViewCollection
	| ManifestBase;
