import type { ManifestAuthProvider } from './auth-provider.model.js';
import type { ManifestCurrentUserAction, ManifestCurrentUserActionDefaultKind } from './current-user-action.model.js';
import type { ManifestDashboard } from './dashboard.model.js';
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
import type { ManifestFileUploadPreview } from './file-upload-preview.model.js';
import type { ManifestExternalLoginProvider } from './external-login-provider.model.js';
import type { ManifestGlobalContext } from './global-context.model.js';
import type { ManifestHeaderApp, ManifestHeaderAppButtonKind } from './header-app.model.js';
import type { ManifestHealthCheck } from './health-check.model.js';
import type { ManifestIcons } from './icons.model.js';
import type { ManifestLocalization } from './localization.model.js';
import type { ManifestMenu } from './menu.model.js';
import type { ManifestMenuItem, ManifestMenuItemLinkKind, ManifestMenuItemTreeKind } from './menu-item.model.js';
import type { ManifestPreviewAppProvider } from './preview-app.model.js';
import type { ManifestPropertyAction, ManifestPropertyActionDefaultKind } from './property-action.model.js';
import type { ManifestPropertyEditorUi, ManifestPropertyEditorSchema } from './property-editor.model.js';
import type { ManifestRepository } from './repository.model.js';
import type { ManifestSection } from './section.model.js';
import type { ManifestSectionSidebarApp, ManifestSectionSidebarAppMenuKind } from './section-sidebar-app.model.js';
import type { ManifestSectionView } from './section-view.model.js';
import type { ManifestStore, ManifestTreeStore, ManifestItemStore } from './store.model.js';
import type { ManifestTinyMcePlugin } from './tinymce-plugin.model.js';
import type { ManifestUfmComponent } from './ufm-component.model.js';
import type { ManifestUfmFilter } from './ufm-filter.model.js';
import type { ManifestUserProfileApp } from './user-profile-app.model.js';
import type { ManifestEntityUserPermission } from './entity-user-permission.model.js';
import type { ManifestGranularUserPermission } from './user-granular-permission.model.js';
import type { ManifestMfaLoginProvider } from './mfa-login-provider.model.js';
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
export type * from './current-user-action.model.js';
export type * from './dashboard.model.js';
export type * from './dynamic-root.model.js';
export type * from './entity-action.model.js';
export type * from './entity-bulk-action.model.js';
export type * from './entity-user-permission.model.js';
export type * from './entry-point.model.js';
export type * from './file-upload-preview.model.js';
export type * from './external-login-provider.model.js';
export type * from './global-context.model.js';
export type * from './header-app.model.js';
export type * from './health-check.model.js';
export type * from './icons.model.js';
export type * from './localization.model.js';
export type * from './menu-item.model.js';
export type * from './menu.model.js';
export type * from './mfa-login-provider.model.js';
export type * from './monaco-markdown-editor-action.model.js';
export type * from './picker-search-result-item.model.js';
export type * from './preview-app.model.js';
export type * from './property-action.model.js';
export type * from './property-editor.model.js';
export type * from './repository.model.js';
export type * from './section-sidebar-app.model.js';
export type * from './section-view.model.js';
export type * from './section.model.js';
export type * from './store.model.js';
export type * from './tinymce-plugin.model.js';
export type * from './ufm-component.model.js';
export type * from './ufm-filter.model.js';
export type * from './user-granular-permission.model.js';
export type * from './user-profile-app.model.js';

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

export type ManifestPropertyActions = ManifestPropertyAction | ManifestPropertyActionDefaultKind;

export type ManifestTypes =
	| ManifestAppEntryPoint
	| ManifestAuthProvider
	| ManifestBackofficeEntryPoint
	| ManifestBundle<ManifestTypes>
	| ManifestCondition
	| ManifestCurrentUserAction
	| ManifestCurrentUserActionDefaultKind
	| ManifestDashboard
	| ManifestDynamicRootOrigin
	| ManifestDynamicRootQueryStep
	| ManifestEntityActions
	| ManifestEntityBulkAction
	| ManifestEntityUserPermission
	| ManifestEntryPoint
	| ManifestFileUploadPreview
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
	| ManifestMenuItemLinkKind
	| ManifestMfaLoginProvider
	| ManifestMonacoMarkdownEditorAction
	| ManifestPickerSearchResultItem
	| ManifestPreviewAppProvider
	| ManifestPropertyActions
	| ManifestPropertyEditorSchema
	| ManifestPropertyEditorUi
	| ManifestRepository
	| ManifestSection
	| ManifestSectionRoute
	| ManifestSectionSidebarApp
	| ManifestSectionSidebarAppMenuKind
	| ManifestSectionView
	| ManifestStore
	| ManifestTinyMcePlugin
	| ManifestTreeStore
	| ManifestUfmComponent
	| ManifestUfmFilter
	| ManifestUserProfileApp
	| ManifestBase;

type UnionOfProperties<T> = T extends object ? T[keyof T] : never;

declare global {
	/**
	 * This global type allows to declare manifests types from its own module.
	 * @example
	 ```js
 	 	declare global {
 	 		interface UmbExtensionManifestMap {
 	 			My_UNIQUE_MANIFEST_NAME: MyExtensionManifestType;
  		}
  	}
  	```
	 If you have multiple types, you can declare them in this way:
	 ```js
		declare global {
			interface UmbExtensionManifestMap {
				My_UNIQUE_MANIFEST_NAME: MyExtensionManifestTypeA | MyExtensionManifestTypeB;
			}
		}
	 ```
	 */
	interface UmbExtensionManifestMap {
		UMB_CORE: ManifestTypes;
	}

	/**
	 * This global type provides a union of all declared manifest types.
	 * If this is a local package that declares additional Manifest Types, then these will also be included in this union.
	 */
	type UmbExtensionManifest = UnionOfProperties<UmbExtensionManifestMap>;
}
