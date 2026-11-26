import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * A group of related actions in an action menu. Actions reference it by alias through their `group` property and
 * always render together between separators, ordered by their own weight. The group's weight orders it in the menu.
 */
export interface ManifestActionGroupBase extends ManifestBase {
	meta: MetaActionGroup;
}

/**
 * The metadata of an action group.
 */
export interface MetaActionGroup {
	/**
	 * The accessible name of the group, read by assistive technology. Plain text or a localization key.
	 * @examples ["#actionGroups_publishing", "Publishing"]
	 */
	label: string;
}
