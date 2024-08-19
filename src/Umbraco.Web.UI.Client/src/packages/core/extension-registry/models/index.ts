import type { ManifestAuthProvider } from './auth-provider.model.js';
import type { ManifestBlockEditorCustomView } from './block-editor-custom-view.model.js';
import type { ManifestCollection } from './collection.models.js';
import type { ManifestCollectionView } from './collection-view.model.js';
import type { ManifestCurrentUserAction, ManifestCurrentUserActionDefaultKind } from './current-user-action.model.js';
import type { ManifestDashboard } from './dashboard.model.js';
import type { ManifestDashboardCollection } from './dashboard-collection.model.js';
import type {
	ManifestEntityAction,
	ManifestEntityActionDeleteKind,
	ManifestEntityActionRenameServerFileKind,
	ManifestEntityActionReloadTreeItemChildrenKind,
	ManifestEntityActionDuplicateToKind,
	ManifestEntityActionMoveToKind,
	ManifestEntityActionCreateFolderKind,
	ManifestEntityActionUpdateFolderKind,
	ManifestEntityActionDeleteFolderKind,
	ManifestEntityActionDefaultKind,
	ManifestEntityActionTrashKind,
	ManifestEntityActionRestoreFromRecycleBinKind,
	ManifestEntityActionEmptyRecycleBinKind,
	ManifestEntityActionSortChildrenOfKind,
} from './entity-action.model.js';
import type { ManifestDynamicRootOrigin, ManifestDynamicRootQueryStep } from './dynamic-root.model.js';
import type { ManifestEntityBulkAction } from './entity-bulk-action.model.js';
import type { ManifestExternalLoginProvider } from './external-login-provider.model.js';
import type { ManifestGlobalContext } from './global-context.model.js';
import type { ManifestHeaderApp, ManifestHeaderAppButtonKind } from './header-app.model.js';
import type { ManifestHealthCheck } from './health-check.model.js';
import type { ManifestIcons } from './icons.model.js';
import type { ManifestLocalization } from './localization.model.js';
import type { ManifestMenu } from './menu.model.js';
import type { ManifestMenuItem, ManifestMenuItemTreeKind } from './menu-item.model.js';
import type { ManifestModal } from './modal.model.js';
import type { ManifestPackageView } from './package-view.model.js';
import type { ManifestPreviewAppProvider } from './preview-app.model.js';
import type { ManifestPropertyAction, ManifestPropertyActionDefaultKind } from './property-action.model.js';
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
import type { ManifestUfmComponent } from './ufm-component.model.js';
import type { ManifestUserProfileApp } from './user-profile-app.model.js';
import type { ManifestWorkspace, ManifestWorkspaceRoutableKind } from './workspace.model.js';
import type { ManifestWorkspaceAction, ManifestWorkspaceActionDefaultKind } from './workspace-action.model.js';
import type { ManifestWorkspaceActionMenuItem } from './workspace-action-menu-item.model.js';
import type { ManifestWorkspaceContext } from './workspace-context.model.js';
import type {
	ManifestWorkspaceFooterApp,
	ManifestWorkspaceFooterAppMenuBreadcrumbKind,
	ManifestWorkspaceFooterAppVariantMenuBreadcrumbKind,
} from './workspace-footer-app.model.js';
import type {
	ManifestWorkspaceView,
	ManifestWorkspaceViewContentTypeDesignEditorKind,
} from './workspace-view.model.js';
import type { ManifestEntityUserPermission } from './entity-user-permission.model.js';
import type { ManifestGranularUserPermission } from './user-granular-permission.model.js';
import type { ManifestCollectionAction } from './collection-action.model.js';
import type { ManifestMfaLoginProvider } from './mfa-login-provider.model.js';
import type { ManifestSearchProvider } from './search-provider.model.js';
import type { ManifestSearchResultItem } from './search-result-item.model.js';
import type { ManifestAppEntryPoint } from './app-entry-point.model.js';
import type { ManifestBackofficeEntryPoint } from './backoffice-entry-point.model.js';
import type { ManifestEntryPoint } from './entry-point.model.js';
import type { ManifestMonacoMarkdownEditorAction } from './monaco-markdown-editor-action.model.js';
import type { ManifestSectionRoute } from './section-route.model.js';
import type { ManifestPickerSearchResultItem } from './picker-search-result-item.model.js';
import type { ManifestBase, ManifestBundle, ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export type * from './app-entry-point.model.js';
export type * from './auth-provider.model.js';
export type * from './backoffice-entry-point.model.js';
export type * from './block-editor-custom-view.model.js';
export type * from './collection-action.model.js';
export type * from './collection-view.model.js';
export type * from './collection.models.js';
export type * from './current-user-action.model.js';
export type * from './dashboard-collection.model.js';
export type * from './dashboard.model.js';
export type * from './dynamic-root.model.js';
export type * from './entity-action.model.js';
export type * from './entity-bulk-action.model.js';
export type * from './entity-user-permission.model.js';
export type * from './entry-point.model.js';
export type * from './external-login-provider.model.js';
export type * from './global-context.model.js';
export type * from './header-app.model.js';
export type * from './health-check.model.js';
export type * from './icons.model.js';
export type * from './localization.model.js';
export type * from './menu-item.model.js';
export type * from './menu.model.js';
export type * from './mfa-login-provider.model.js';
export type * from './modal.model.js';
export type * from './monaco-markdown-editor-action.model.js';
export type * from './package-view.model.js';
export type * from './picker-search-result-item.model.js';
export type * from './preview-app.model.js';
export type * from './property-action.model.js';
export type * from './property-editor.model.js';
export type * from './repository.model.js';
export type * from './search-provider.model.js';
export type * from './search-result-item.model.js';
export type * from './section-sidebar-app.model.js';
export type * from './section-view.model.js';
export type * from './section.model.js';
export type * from './store.model.js';
export type * from './theme.model.js';
export type * from './tinymce-plugin.model.js';
export type * from './tree-item.model.js';
export type * from './tree.model.js';
export type * from './ufm-component.model.js';
export type * from './user-granular-permission.model.js';
export type * from './user-profile-app.model.js';
export type * from './workspace-action-menu-item.model.js';
export type * from './workspace-action.model.js';
export type * from './workspace-context.model.js';
export type * from './workspace-footer-app.model.js';
export type * from './workspace-view.model.js';
export type * from './workspace.model.js';

export type ManifestEntityActions =
	| ManifestEntityAction
	| ManifestEntityActionCreateFolderKind
	| ManifestEntityActionDefaultKind
	| ManifestEntityActionDeleteFolderKind
	| ManifestEntityActionDeleteKind
	| ManifestEntityActionDuplicateToKind
	| ManifestEntityActionEmptyRecycleBinKind
	| ManifestEntityActionMoveToKind
	| ManifestEntityActionReloadTreeItemChildrenKind
	| ManifestEntityActionRenameServerFileKind
	| ManifestEntityActionRestoreFromRecycleBinKind
	| ManifestEntityActionSortChildrenOfKind
	| ManifestEntityActionTrashKind
	| ManifestEntityActionUpdateFolderKind;

export type ManifestWorkspaceFooterApps =
	| ManifestWorkspaceFooterApp
	| ManifestWorkspaceFooterAppMenuBreadcrumbKind
	| ManifestWorkspaceFooterAppVariantMenuBreadcrumbKind;

export type ManifestPropertyActions = ManifestPropertyAction | ManifestPropertyActionDefaultKind;

export type ManifestWorkspaceActions = ManifestWorkspaceAction | ManifestWorkspaceActionDefaultKind;

export type ManifestWorkspaces = ManifestWorkspace | ManifestWorkspaceRoutableKind | ManifestWorkspaceRoutableKind;
export type ManifestWorkspaceViews = ManifestWorkspaceView | ManifestWorkspaceViewContentTypeDesignEditorKind;

export type ManifestTypes =
	| ManifestAppEntryPoint
	| ManifestAuthProvider
	| ManifestBackofficeEntryPoint
	| ManifestBlockEditorCustomView
	| ManifestBundle<ManifestTypes>
	| ManifestCollection
	| ManifestCollectionAction
	| ManifestCollectionView
	| ManifestCondition
	| ManifestCurrentUserAction
	| ManifestCurrentUserActionDefaultKind
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestDynamicRootOrigin
	| ManifestDynamicRootQueryStep
	| ManifestEntityActions
	| ManifestEntityBulkAction
	| ManifestEntityUserPermission
	| ManifestEntryPoint
	| ManifestExternalLoginProvider
	| ManifestGlobalContext
	| ManifestGranularUserPermission
	| ManifestHeaderApp
	| ManifestHeaderAppButtonKind
	| ManifestHealthCheck
	| ManifestIcons
	| ManifestItemStore
	| ManifestLocalization
	| ManifestMenu
	| ManifestMenuItem
	| ManifestMenuItemTreeKind
	| ManifestMfaLoginProvider
	| ManifestModal
	| ManifestMonacoMarkdownEditorAction
	| ManifestPackageView
	| ManifestPickerSearchResultItem
	| ManifestPreviewAppProvider
	| ManifestPropertyActions
	| ManifestPropertyEditorSchema
	| ManifestPropertyEditorUi
	| ManifestRepository
	| ManifestSearchProvider
	| ManifestSearchResultItem
	| ManifestSection
	| ManifestSectionRoute
	| ManifestSectionSidebarApp
	| ManifestSectionSidebarAppMenuKind
	| ManifestSectionView
	| ManifestStore
	| ManifestTheme
	| ManifestTinyMcePlugin
	| ManifestTree
	| ManifestTreeItem
	| ManifestTreeStore
	| ManifestUfmComponent
	| ManifestUserProfileApp
	| ManifestWorkspaceActionMenuItem
	| ManifestWorkspaceActions
	| ManifestWorkspaceContext
	| ManifestWorkspaceFooterApps
	| ManifestWorkspaces
	| ManifestWorkspaceViews
	| ManifestBase;
