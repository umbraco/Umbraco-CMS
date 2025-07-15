import type {
	ManifestWorkspaceActionMenuItem,
	ManifestWorkspaceActionMenuItemDefaultKind,
} from './workspace-action-menu-item.model.js';
import type { ManifestWorkspaceAction, ManifestWorkspaceActionDefaultKind } from './workspace-action.model.js';
import type {
	ManifestWorkspaceFooterApp,
	ManifestWorkspaceFooterAppMenuBreadcrumbKind,
	ManifestWorkspaceFooterAppVariantMenuBreadcrumbKind,
} from './workspace-footer-app.model.js';
import type {
	ManifestWorkspaceView,
	ManifestWorkspaceViewContentTypeDesignEditorKind,
} from './workspace-view.model.js';
import type { ManifestWorkspace, ManifestWorkspaceRoutableKind } from './workspace.model.js';

export type * from './workspace-action-menu-item.model.js';
export type * from './workspace-action.model.js';
export type * from './workspace-context.model.js';
export type * from './workspace-footer-app.model.js';
export type * from './workspace-view.model.js';
export type * from './workspace.model.js';

export type ManifestWorkspaceFooterApps =
	| ManifestWorkspaceFooterApp
	| ManifestWorkspaceFooterAppMenuBreadcrumbKind
	| ManifestWorkspaceFooterAppVariantMenuBreadcrumbKind;

export type ManifestWorkspaceActions = ManifestWorkspaceAction | ManifestWorkspaceActionDefaultKind;
export type ManifestWorkspaceMenuItemActions =
	| ManifestWorkspaceActionMenuItem
	| ManifestWorkspaceActionMenuItemDefaultKind;

export type ManifestWorkspaces = ManifestWorkspace | ManifestWorkspaceRoutableKind;
export type ManifestWorkspaceViews = ManifestWorkspaceView | ManifestWorkspaceViewContentTypeDesignEditorKind;
