import type { ManifestActionGroupBase } from './action-group.extension.js';
import { compareExtensionsByWeight } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action (or its extension controller) that can be ordered by `umbOrderActionsByGroup`.
 */
export interface UmbGroupableAction {
	alias: string;
	weight?: number;
	manifest?: { group?: string; separatorBefore?: boolean };
}

export interface UmbActionMenuGroup {
	alias: string;
	/** The registered group, or undefined for a fallback group: a `group` alias without a registered group. */
	manifest?: ManifestActionGroupBase;
}

export interface UmbActionMenuEntry<T extends UmbGroupableAction> {
	action: T;
	/** Whether a separator renders before this action. */
	separatorBefore: boolean;
}

export interface UmbActionMenuItem<T extends UmbGroupableAction> {
	/** The group of the actions, or undefined for a single ungrouped action. */
	group?: UmbActionMenuGroup;
	actions: Array<UmbActionMenuEntry<T>>;
}

const warnedUnresolvedGroups = new Set<string>();

/**
 * Orders the visible actions of one menu by their action groups and decides where separators go.
 * Groups (at the group's weight) and ungrouped actions (at their own weight) are sorted together, highest weight first;
 * at equal weight a group comes before an ungrouped action, then by alias. The actions of a group are sorted by their
 * own weight, then by alias. Actions that reference an unregistered group are kept together in a fallback group per
 * alias, after everything else. Every group gets a separator on both edges; `separatorBefore` adds one before its
 * action, also inside a group. There is never a separator first, last or twice in a row.
 * @param {ReadonlyArray<T>} actions - The visible actions of the menu.
 * @param {ReadonlyArray<ManifestActionGroupBase>} groups - The registered groups of the menu's action type.
 * @param {string} actionType - The extension type of the actions, used in the warning for an unregistered group.
 * @returns {Array<UmbActionMenuItem<T>>} The menu items: groups and ungrouped actions, in render order.
 */
export function umbOrderActionsByGroup<T extends UmbGroupableAction>(
	actions: ReadonlyArray<T>,
	groups: ReadonlyArray<ManifestActionGroupBase>,
	actionType: string,
): Array<UmbActionMenuItem<T>> {
	const groupsByAlias = new Map(groups.map((group) => [group.alias, group]));
	const membersByGroup = new Map<string, Array<T>>();
	const ungrouped: Array<T> = [];

	for (const action of actions) {
		const groupAlias = action.manifest?.group;
		if (!groupAlias) {
			ungrouped.push(action);
			continue;
		}

		let members = membersByGroup.get(groupAlias);
		if (!members) membersByGroup.set(groupAlias, (members = []));
		members.push(action);
	}

	const toEntries = (members: Array<T>) =>
		[...members].sort(compareExtensionsByWeight).map((action) => ({ action, separatorBefore: false }));

	type Sortable = { alias: string; weight: number; isGroup: boolean; item: UmbActionMenuItem<T> };
	const sortables: Array<Sortable> = [];
	const fallbacks: Array<UmbActionMenuItem<T>> = [];

	for (const [alias, members] of membersByGroup) {
		const manifest = groupsByAlias.get(alias);
		const item: UmbActionMenuItem<T> = { group: { alias, manifest }, actions: toEntries(members) };
		if (manifest) {
			sortables.push({ alias, weight: manifest.weight ?? 0, isGroup: true, item });
		} else {
			warnUnresolvedGroup(actionType, alias);
			fallbacks.push(item);
		}
	}

	for (const action of ungrouped) {
		sortables.push({
			alias: action.alias,
			weight: action.weight ?? 0,
			isGroup: false,
			item: { actions: [{ action, separatorBefore: false }] },
		});
	}

	sortables.sort(
		(a, b) => b.weight - a.weight || Number(b.isGroup) - Number(a.isGroup) || compareExtensionsByWeight(a, b),
	);
	fallbacks.sort((a, b) => (a.group!.alias < b.group!.alias ? -1 : 1));

	const items = [...sortables.map((x) => x.item), ...fallbacks];

	items.forEach((item, index) => {
		item.actions.forEach((entry, entryIndex) => {
			if (entryIndex > 0) {
				entry.separatorBefore = entry.action.manifest?.separatorBefore === true;
			} else if (index > 0) {
				entry.separatorBefore =
					item.group !== undefined ||
					items[index - 1].group !== undefined ||
					entry.action.manifest?.separatorBefore === true;
			}
		});
	});

	return items;
}

/**
 * Warns once per action type and alias that actions reference a group that is not registered.
 * @param {string} actionType - The extension type of the actions.
 * @param {string} alias - The unregistered group alias.
 */
function warnUnresolvedGroup(actionType: string, alias: string) {
	const key = `${actionType}:${alias}`;
	if (warnedUnresolvedGroups.has(key)) return;
	warnedUnresolvedGroups.add(key);
	console.warn(
		`Actions of type "${actionType}" reference the action group "${alias}", which is not registered. They are rendered in a fallback group at the end of the menu.`,
	);
}
