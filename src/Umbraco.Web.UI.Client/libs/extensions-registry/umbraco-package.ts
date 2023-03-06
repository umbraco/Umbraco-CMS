import type {
	ManifestCollectionView,
	ManifestCustom,
	ManifestDashboard,
	ManifestDashboardCollection,
	ManifestEntityAction,
	ManifestEntityBulkAction,
	ManifestEntrypoint,
	ManifestExternalLoginProvider,
	ManifestHeaderApp,
	ManifestHealthCheck,
	ManifestMenu,
	ManifestMenuItem,
	ManifestMenuSectionSidebarApp,
	ManifestPackageView,
	ManifestPropertyAction,
	ManifestPropertyEditorModel,
	ManifestPropertyEditorUI,
	ManifestRepository,
	ManifestSection,
	ManifestSectionSidebarApp,
	ManifestSectionView,
	ManifestTheme,
	ManifestUserDashboard,
	ManifestWorkspace,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from './models';

export type ManifestJSONTypes =
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
	| ManifestSectionSidebarApp
	| ManifestSectionView
	| ManifestMenuSectionSidebarApp
	| ManifestMenu
	| ManifestMenuItem
	| ManifestTheme
	| ManifestUserDashboard
	| ManifestWorkspace
	| ManifestWorkspaceView
	| ManifestWorkspaceViewCollection;

export class UmbracoPackage {
	name?: string;
	version?: string;
	extensions?: ManifestJSONTypes[];
}
