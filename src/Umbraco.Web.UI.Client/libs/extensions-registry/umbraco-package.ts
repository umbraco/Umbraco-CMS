import type {
	ManifestCollectionView,
	ManifestDashboard,
	ManifestDashboardCollection,
	ManifestEntityAction,
	ManifestEntityBulkAction,
	ManifestEntrypoint,
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
	| ManifestDashboard
	| ManifestDashboardCollection
	| ManifestEntityAction
	| ManifestEntityBulkAction
	| ManifestEntrypoint
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

type LoadableManifestJSONTypes = ManifestJSONTypes & {
	/**
	 * @title The file location of the javascript file to load
	 */
	js: string;

	/**
	 * @title The name of the exported custom element to use
	 * @description This is optional but useful if your module exports more than one custom element
	 * @example my-dashboard
	 */
	elementName?: string;
};

/**
 * Umbraco package manifest JSON
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
	 * @title Decides if the package sends telemetry data for collection
	 * @default true
	 */
	allowTelemetry?: boolean;

	/**
	 * @title An array of Umbraco package manifest types that will be installed
	 */
	extensions?: LoadableManifestJSONTypes[];
}
