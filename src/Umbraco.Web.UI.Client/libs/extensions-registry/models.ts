import type { ManifestCollectionView } from './collection-view.models';
import type { ManifestDashboard } from './dashboard.models';
import type { ManifestDashboardCollection } from './dashboard-collection.models';
import type { ManifestEntityAction } from './entity-action.models';
import type { ManifestEntityBulkAction } from './entity-bulk-action.models';
import type { ManifestExternalLoginProvider } from './external-login-provider.models';
import type { ManifestHeaderApp } from './header-app.models';
import type { ManifestHealthCheck } from './health-check.models';
import type { ManifestPackageView } from './package-view.models';
import type { ManifestPropertyAction } from './property-action.models';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.models';
import type { ManifestSection } from './section.models';
import type { ManifestSectionView } from './section-view.models';
import type { ManifestSectionSidebarApp, ManifestMenuSectionSidebarApp } from './section-sidebar-app.models';
import type { ManifestMenu } from './menu.models';
import type { ManifestMenuItem } from './menu-item.models';
import type { ManifestTheme } from './theme.models';
import type { ManifestTree } from './tree.models';
import type { ManifestUserDashboard } from './user-dashboard.models';
import type { ManifestWorkspace } from './workspace.models';
import type { ManifestWorkspaceAction } from './workspace-action.models';
import type { ManifestWorkspaceView } from './workspace-view.models';
import type { ManifestWorkspaceViewCollection } from './workspace-view-collection.models';
import type { ManifestRepository } from './repository.models';
import type { ManifestModal } from './modal.models';
import type { ManifestStore, ManifestTreeStore } from './store.models';

export * from './collection-view.models';
export * from './dashboard-collection.models';
export * from './dashboard.models';
export * from './entity-action.models';
export * from './entity-bulk-action.models';
export * from './external-login-provider.models';
export * from './header-app.models';
export * from './health-check.models';
export * from './package-view.models';
export * from './property-action.models';
export * from './property-editor.models';
export * from './section-view.models';
export * from './section.models';
export * from './section-sidebar-app.models';
export * from './menu.models';
export * from './menu-item.models';
export * from './theme.models';
export * from './tree.models';
export * from './user-dashboard.models';
export * from './workspace-action.models';
export * from './workspace-view-collection.models';
export * from './workspace-view.models';
export * from './repository.models';
export * from './store.models';
export * from './workspace.models';
export * from './modal.models';

export type ManifestTypes =
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
	| ManifestTree
	| ManifestUserDashboard
	| ManifestWorkspace
	| ManifestWorkspaceAction
	| ManifestWorkspaceView
	| ManifestWorkspaceViewCollection
	| ManifestModal
	| ManifestStore
	| ManifestTreeStore
	| ManifestBase;

export type ManifestStandardTypes = ManifestTypes['type'];

export type ManifestTypeMap = {
	[Manifest in ManifestTypes as Manifest['type']]: Manifest;
};

export interface ManifestBase {
	/**
	 * The type of extension such as dashboard etc...
	 */
	type: string;

	/**
	 * The alias of the extension, ensure it is unique
	 */
	alias: string;

	/**
	 * The friendly name of the extension
	 */
	name: string;

	/**
	 * Extensions such as dashboards are ordered by weight with lower numbers being first in the list
	 */
	weight?: number;
}

export interface ManifestWithLoader<LoaderReturnType> extends ManifestBase {
	/**
	 * Ignore this property when serializing to JSON Schema
	 * found at /types/umbraco-package-schema.json
	 * @ignore
	 */
	loader?: () => Promise<LoaderReturnType>;
}

/**
 * The type of extension such as dashboard etc...
 */
export interface ManifestClass<T = unknown> extends ManifestWithLoader<object> {
	type: ManifestStandardTypes;

	/**
	 * The file location of the javascript file to load
	 */
	js?: string;

	className?: string;
	class?: ClassConstructor<T>;
	//loader?: () => Promise<object | HTMLElement>;
}

export interface ManifestClassWithClassConstructor extends ManifestClass {
	class: ClassConstructor<unknown>;
}

export interface ManifestElement extends ManifestWithLoader<object | HTMLElement> {
	/**
	 * The type of extension such as dashboard etc...
	 */
	type: ManifestStandardTypes;

	/**
	 * The file location of the javascript file to load
	 */
	js?: string;

	/**
	 * The HTML web component name to use such as 'my-dashboard'
	 * Note it is NOT <my-dashboard></my-dashboard> but just the name
	 */
	elementName?: string;

	/**
	 * This contains properties specific to the type of extension
	 */
	meta?: unknown;
}

export interface ManifestWithView extends ManifestElement {
	meta: MetaManifestWithView;
}

export interface MetaManifestWithView {
	pathname: string;
	label: string;
	icon: string;
}

export interface ManifestElementWithElementName extends ManifestElement {
	/**
	 * The HTML web component name to use such as 'my-dashboard'
	 * Note it is NOT <my-dashboard></my-dashboard> but just the name
	 */
	elementName: string;
}

export interface ManifestCustom extends ManifestBase {
	type: 'custom';

	/**
	 * This contains properties specific to the type of extension
	 */
	meta?: unknown;
}

export interface ManifestWithMeta extends ManifestBase {
	/**
	 * This contains properties specific to the type of extension
	 */
	meta: unknown;
}

export type ClassConstructor<T> = new (...args: any[]) => T;

/**
 * This type of extension gives full control and will simply load the specified JS file
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry
 */
export interface ManifestEntrypoint extends ManifestBase {
	type: 'entrypoint';

	/**
	 * The file location of the javascript file to load in the backoffice
	 */
	js: string;
}
