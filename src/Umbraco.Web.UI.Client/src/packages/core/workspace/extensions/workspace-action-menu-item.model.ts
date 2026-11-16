import type { UmbWorkspaceActionMenuItem } from '../components/workspace-action-menu-item/workspace-action-menu-item.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceActionMenuItem<
	MetaType extends MetaWorkspaceActionMenuItem = MetaWorkspaceActionMenuItem,
>
	extends
		ManifestElementAndApi<UmbControllerHostElement, UmbWorkspaceActionMenuItem<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'workspaceActionMenuItem';
	/**
	 * Define which workspace actions this menu item should be shown for.
	 * @examples [["Umb.WorkspaceAction.Document.Save", "Umb.WorkspaceAction.Document.SaveAndPublish"], "Umb.WorkspaceAction.Document.Save"]
	 * @required
	 */
	forWorkspaceActions: string | string[];
	/**
	 * The alias of the action group this action belongs to. The actions of a group render together, ordered by their
	 * own weight, and the group's weight orders the group in the menu. Actions without a registered group render last.
	 * @examples ["Umb.WorkspaceActionMenuItemGroup.Publishing"]
	 */
	group?: string;
	/**
	 * Renders a separator above this action inside its group. The first action of a group needs none, as a separator
	 * already sits between groups.
	 * The separator belongs to this action, so it is not rendered when the action is not.
	 */
	separatorBefore?: boolean;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceActionMenuItem {}

export interface ManifestWorkspaceActionMenuItemDefaultKind<
	MetaType extends MetaWorkspaceActionMenuItemDefaultKind = MetaWorkspaceActionMenuItemDefaultKind,
> extends ManifestWorkspaceActionMenuItem<MetaType> {
	type: 'workspaceActionMenuItem';
	kind: 'default';
}

export interface MetaWorkspaceActionMenuItemDefaultKind extends MetaWorkspaceActionMenuItem {
	/**
	 * An icon to represent the action to be performed
	 * @examples ["icon-box", "icon-grid"]
	 */
	icon: string;

	/**
	 * The friendly name of the action to perform
	 * @examples ["Create", "Create Content Template"]
	 */
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestWorkspaceActionMenuItem: ManifestWorkspaceActionMenuItem;
		ManifestWorkspaceActionMenuItemDefaultKind: ManifestWorkspaceActionMenuItemDefaultKind;
	}
}
