import type { ManifestBlockEditorCustomView } from './block-editor-custom-view.model.js';
import type { ManifestCollection } from './collection.models.js';
import type { ManifestCollectionView } from './collection-view.model.js';
import type { ManifestDashboard } from './dashboard.model.js';
import type { ManifestDashboardCollection } from './dashboard-collection.model.js';
import type {
	ManifestEntityAction,
	ManifestEntityActionDeleteKind,
	ManifestEntityActionRenameKind,
	ManifestEntityActionReloadTreeItemChildrenKind,
	ManifestEntityActionDuplicateKind,
	ManifestEntityActionMoveKind,
} from './entity-action.model.js';
import type { ManifestDynamicRootOrigin, ManifestDynamicRootQueryStep } from './dynamic-root.model.js';
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
import type { ManifestLocalization } from './localization.model.js';
import type { ManifestTree } from './tree.model.js';
import type { ManifestTreeItem } from './tree-item.model.js';
import type { ManifestUserProfileApp } from './user-profile-app.model.js';
import type { ManifestWorkspace } from './workspace.model.js';
import type { ManifestWorkspaceAction } from './workspace-action.model.js';
import type { ManifestWorkspaceActionMenuItem } from './workspace-action-menu-item.model.js';
import type { ManifestWorkspaceContext } from './workspace-context.model.js';
import type { ManifestWorkspaceFooterApp } from './workspace-footer-app.model.js';
import type { ManifestWorkspaceView } from './workspace-view.model.js';
import type { ManifestEntityUserPermission } from './entity-user-permission.model.js';
import type { ManifestGranularUserPermission } from './user-granular-permission.model.js';
import type { ManifestCollectionAction } from './collection-action.model.js';
import type {
	ManifestBase,
	ManifestBundle,
	ManifestCondition,
	ManifestEntryPoint,
} from '@umbraco-cms/backoffice/extension-api';

export type * from './block-editor-custom-view.model.js';
export type * from './collection.models.js';
export type * from './collection-action.model.js';
export type * from './collection-view.model.js';
export type * from './dashboard-collection.model.js';
export type * from './dashboard.model.js';
export type * from './dynamic-root.model.js';
export type * from './entity-action.model.js';
export type * from './entity-bulk-action.model.js';
export type * from './external-login-provider.model.js';
export type * from './global-context.model.js';
export type * from './header-app.model.js';
export type * from './health-check.model.js';
export type * from './localization.model.js';
export type * from './menu-item.model.js';
export type * from './menu.model.js';
export type * from './modal.model.js';
export type * from './package-view.model.js';
export type * from './property-action.model.js';
export type * from './property-editor.model.js';
export type * from './repository.model.js';
export type * from './section-sidebar-app.model.js';
export type * from './section-view.model.js';
export type * from './section.model.js';
export type * from './store.model.js';
export type * from './theme.model.js';
export type * from './tinymce-plugin.model.js';
export type * from './tree-item.model.js';
export type * from './tree.model.js';
export type * from './user-granular-permission.model.js';
export type * from './entity-user-permission.model.js';
export type * from './user-profile-app.model.js';
export type * from './workspace-action.model.js';
export type * from './workspace-action-menu-item.model.js';
export type * from './workspace-context.model.js';
export type * from './workspace-footer-app.model.js';
export type * from './workspace-view.model.js';
export type * from './workspace.model.js';

export type ManifestTypes =
	| ManifestBundle<ManifestTypes>
	| ManifestCondition
	| ManifestBlockEditorCustomView
	| ManifestCollection
	| ManifestCollectionView
	| ManifestCollectionAction
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestDynamicRootOrigin
	| ManifestDynamicRootQueryStep
	| ManifestEntityAction
	| ManifestEntityActionDeleteKind
	| ManifestEntityActionRenameKind
	| ManifestEntityActionReloadTreeItemChildrenKind
	| ManifestEntityActionDuplicateKind
	| ManifestEntityActionMoveKind
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
	| ManifestLocalization
	| ManifestTree
	| ManifestTreeItem
	| ManifestTreeStore
	| ManifestUserProfileApp
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceActionMenuItem
	| ManifestWorkspaceContext
	| ManifestWorkspaceFooterApp
	| ManifestWorkspaceView
	| ManifestEntityUserPermission
	| ManifestGranularUserPermission
	| ManifestBase;
