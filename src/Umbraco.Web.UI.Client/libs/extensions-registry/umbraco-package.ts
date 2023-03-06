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

/**
 * Umbraco package manifest JSON
 * @additionalProperties false
 */
export class UmbracoPackage {

	/**
	 * @title The name of the Umbraco package
	 */
	name?: string;

	/**
	 * @title The version of the Umbraco package
	 */
	version?: string;

	/**
	 * @title An array of Umbraco package manifest types that will be installed
	 */
	extensions?: ManifestJSONTypes[];
}
