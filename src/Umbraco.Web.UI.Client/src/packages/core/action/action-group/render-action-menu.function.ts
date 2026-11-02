import type { UmbActionMenuGroup, UmbActionMenuItem, UmbGroupableAction } from './order-actions-by-group.function.js';
import { html, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

const separator = () => html`<umb-separator></umb-separator>`;

/**
 * Renders menu items ordered by `umbOrderActionsByGroup`: each group is wrapped in an element with the `group` role and
 * the group's label as its accessible name, and separators render where the items say so.
 * @param {Array<UmbActionMenuItem<T>>} items - The ordered menu items.
 * @param {(action: T) => unknown} renderAction - Renders one action.
 * @param {UmbLocalizationController} localize - Localizes the group labels.
 * @returns {unknown} The rendered menu.
 */
export function umbRenderActionMenu<T extends UmbGroupableAction>(
	items: Array<UmbActionMenuItem<T>>,
	renderAction: (action: T) => unknown,
	localize: UmbLocalizationController,
) {
	return repeat(
		items,
		(item) => (item.group ? `group:${item.group.alias}` : item.actions[0].action.alias),
		(item) => {
			const [first] = item.actions;
			const before = first.separatorBefore ? separator() : nothing;
			if (!item.group) return html`${before}${renderAction(first.action)}`;

			return html`${before}
				<div
					role="group"
					aria-label=${getGroupLabel(item.group, localize)}
					data-mark="action-group:${item.group.alias}">
					${repeat(
						item.actions,
						(entry) => entry.action.alias,
						(entry, index) =>
							html`${index > 0 && entry.separatorBefore ? separator() : nothing}${renderAction(entry.action)}`,
					)}
				</div>`;
		},
	);
}

/**
 * Gets the accessible name of a group: its localized label, or a name with the alias for a fallback group.
 * @param {UmbActionMenuGroup} group - The group.
 * @param {UmbLocalizationController} localize - Localizes the label.
 * @returns {string} The accessible name.
 */
function getGroupLabel(group: UmbActionMenuGroup, localize: UmbLocalizationController): string {
	return group.manifest
		? localize.string(group.manifest.meta.label)
		: localize.term('actionGroups_unregistered', group.alias);
}
