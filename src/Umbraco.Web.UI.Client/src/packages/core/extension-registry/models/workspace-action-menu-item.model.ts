import type { ConditionTypes } from '../conditions/types.js';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceActionMenuItem
	extends ManifestElement<HTMLElement>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceActionMenuItem';
	meta: MetaWorkspaceActionMenuItem;
}

export interface MetaWorkspaceActionMenuItem {
	/**
	 * Define which workspace actions this menu item should be shown for.
	 */
	workspaceActionAliases: string[];

	/**
	 * The color of the button. Defaults to the workspace action button color.
	 */
	buttonColor?: UUIInterfaceColor;

	/**
	 * The look of the button. Defaults to the workspace action button look.
	 */
	buttonLook?: UUIInterfaceLook;
}
