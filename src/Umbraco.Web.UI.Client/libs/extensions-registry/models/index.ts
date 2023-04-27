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

import type { ClassConstructor } from '@umbraco-cms/backoffice/models';

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
	| ManifestEntrypoint
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

export type ManifestStandardTypes = ManifestTypes['type'];

export type ManifestTypeMap = {
	[Manifest in ManifestTypes as Manifest['type']]: Manifest;
};

export type SpecificManifestTypeOrManifestBase<T extends keyof ManifestTypeMap | string> =
	T extends keyof ManifestTypeMap ? ManifestTypeMap[T] : ManifestBase;

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
	 * The kind of the extension, used to group extensions together
	 *
	 * @examples ["button"]
	 */
	kind?: unknown; // I had to add the optional kind property set to undefined. To make the ManifestTypes recognize the Manifest Kind types. Notice that Kinds has to Omit the kind property when extending.

	/**
	 * The friendly name of the extension
	 */
	name: string;

	/**
	 * Extensions such as dashboards are ordered by weight with lower numbers being first in the list
	 */
	weight?: number;
}

export interface ManifestKind {
	type: 'kind';
	alias: string;
	matchType: string;
	matchKind: string;
	manifest: Partial<ManifestTypes>;
}

export interface ManifestWithConditions<ConditionsType> {
	/**
	 * Set the conditions for when the extension should be loaded
	 */
	conditions: ConditionsType;
}

export interface ManifestWithLoader<LoaderReturnType> extends ManifestBase {
	/**
	 * @TJS-ignore
	 */
	loader?: () => Promise<LoaderReturnType>;
}

/**
 * The type of extension such as dashboard etc...
 */
export interface ManifestClass<ClassType = unknown>
	extends ManifestWithLoader<{ default: ClassConstructor<ClassType> }> {
	//type: ManifestStandardTypes;
	readonly CLASS_TYPE?: ClassType;

	/**
	 * The file location of the javascript file to load
	 * @TJS-required
	 */
	js?: string;

	/**
	 * @TJS-ignore
	 */
	className?: string;

	/**
	 * @TJS-ignore
	 */
	class?: ClassConstructor<ClassType>;
	//loader?: () => Promise<object | HTMLElement>;
}

export interface ManifestClassWithClassConstructor<T = unknown> extends ManifestClass<T> {
	class: ClassConstructor<T>;
}

export interface ManifestElement<ElementType extends HTMLElement = HTMLElement>
	extends ManifestWithLoader<{ default: ClassConstructor<ElementType> } | Omit<object, 'default'>> {
	//type: ManifestStandardTypes;
	readonly ELEMENT_TYPE?: ElementType;

	/**
	 * The file location of the javascript file to load
	 *
	 * @TJS-require
	 */
	js?: string;

	/**
	 * The HTML web component name to use such as 'my-dashboard'
	 * Note it is NOT <my-dashboard></my-dashboard> but just the name
	 */
	elementName?: string;

	//loader?: () => Promise<object | HTMLElement>;

	/**
	 * This contains properties specific to the type of extension
	 */
	meta?: unknown;
}

export interface ManifestWithView<ElementType extends HTMLElement = HTMLElement> extends ManifestElement<ElementType> {
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

export interface ManifestWithMeta extends ManifestBase {
	/**
	 * This contains properties specific to the type of extension
	 */
	meta: unknown;
}

/**
 * This type of extension gives full control and will simply load the specified JS file
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry
 */
export interface ManifestEntrypoint extends ManifestBase {
	type: 'entryPoint';

	/**
	 * The file location of the javascript file to load in the backoffice
	 */
	js: string;
}
