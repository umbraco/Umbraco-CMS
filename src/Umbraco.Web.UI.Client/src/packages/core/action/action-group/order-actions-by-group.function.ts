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

/**
 * The action group of a menu item, as returned by `umbOrderActionsByGroup`.
 */
export interface UmbActionMenuGroup {
	alias: string;
	/** The registered group, or undefined for a fallback group: a `group` alias without a registered group. */
	manifest?: ManifestActionGroupBase;
}

/**
 * An action in a menu item, with whether a separator renders before it.
 */
export interface UmbActionMenuEntry<T extends UmbGroupableAction> {
	action: T;
	/** Whether a separator renders before this action. */
	separatorBefore: boolean;
}

/**
 * A top-level item of an ordered action menu: a group with its actions, or a single ungrouped action.
 */
export interface UmbActionMenuItem<T extends UmbGroupableAction> {
	/** The group of the actions, or undefined for a single ungrouped action. */
	group?: UmbActionMenuGroup;
	actions: Array<UmbActionMenuEntry<T>>;
}

type UmbSortableMenuItem<T extends UmbGroupableAction> = {
	alias: string;
	weight: number;
	isGroup: boolean;
	item: UmbActionMenuItem<T>;
};

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
	const actionsByGroup = Map.groupBy(actions, (action) => action.manifest?.group || undefined);
	const sortables = (actionsByGroup.get(undefined) ?? []).map(toUngroupedItem);
	const fallbacks: Array<UmbActionMenuItem<T>> = [];

	for (const [alias, members] of actionsByGroup) {
		if (alias === undefined) continue;
		const manifest = groupsByAlias.get(alias);
		const item: UmbActionMenuItem<T> = { group: { alias, manifest }, actions: toEntries(members) };
		if (manifest) {
			sortables.push({ alias, weight: manifest.weight ?? 0, isGroup: true, item });
		} else {
			warnUnresolvedGroup(actionType, alias);
			fallbacks.push(item);
		}
	}

	sortables.sort(compareSortableMenuItems);
	fallbacks.sort(compareFallbackGroups);
	const items = [...sortables.map((sortable) => sortable.item), ...fallbacks];
	setSeparators(items);
	return items;
}

/**
 * Creates the menu item of an ungrouped action, sortable at the action's own weight.
 * @param {T} action - The ungrouped action.
 * @returns {UmbSortableMenuItem<T>} The sortable menu item.
 */
function toUngroupedItem<T extends UmbGroupableAction>(action: T): UmbSortableMenuItem<T> {
	return {
		alias: action.alias,
		weight: action.weight ?? 0,
		isGroup: false,
		item: { actions: [{ action, separatorBefore: false }] },
	};
}

/**
 * Sorts the actions of a group by weight, then by alias.
 * @param {Array<T>} members - The actions of the group.
 * @returns {Array<UmbActionMenuEntry<T>>} The entries of the group, without separators yet.
 */
function toEntries<T extends UmbGroupableAction>(members: Array<T>): Array<UmbActionMenuEntry<T>> {
	return [...members].sort(compareExtensionsByWeight).map((action) => ({ action, separatorBefore: false }));
}

/**
 * Compares two top-level menu items: highest weight first, then a group before an ungrouped action, then by alias.
 * @param {UmbSortableMenuItem<UmbGroupableAction>} a - The first menu item.
 * @param {UmbSortableMenuItem<UmbGroupableAction>} b - The second menu item.
 * @returns {number} A negative number when `a` sorts first, a positive number when `b` sorts first, otherwise 0.
 */
function compareSortableMenuItems(
	a: UmbSortableMenuItem<UmbGroupableAction>,
	b: UmbSortableMenuItem<UmbGroupableAction>,
): number {
	return b.weight - a.weight || Number(b.isGroup) - Number(a.isGroup) || compareExtensionsByWeight(a, b);
}

/**
 * Compares two fallback groups by alias.
 * @param {UmbActionMenuItem<UmbGroupableAction>} a - The first fallback group.
 * @param {UmbActionMenuItem<UmbGroupableAction>} b - The second fallback group.
 * @returns {number} A negative number when `a` sorts first, otherwise a positive number.
 */
function compareFallbackGroups(
	a: UmbActionMenuItem<UmbGroupableAction>,
	b: UmbActionMenuItem<UmbGroupableAction>,
): number {
	return a.group!.alias < b.group!.alias ? -1 : 1;
}

/**
 * Sets `separatorBefore` on every entry of the ordered menu items.
 * @param {Array<UmbActionMenuItem<T>>} items - The menu items, in render order.
 */
function setSeparators<T extends UmbGroupableAction>(items: Array<UmbActionMenuItem<T>>) {
	items.forEach((item, index) => {
		const previous = items[index - 1];
		item.actions.forEach((entry, entryIndex) => {
			entry.separatorBefore = hasSeparatorBefore(entry, entryIndex === 0, item, previous);
		});
	});
}

/**
 * Decides whether a separator renders before an entry. The first entry of a menu item gets one when a group starts or
 * ends there, or when its action asks for one; any other entry only when its action asks for one. The first entry of
 * the menu never gets one.
 * @param {UmbActionMenuEntry<T>} entry - The entry.
 * @param {boolean} isFirstOfItem - Whether the entry is the first of its menu item.
 * @param {UmbActionMenuItem<T>} item - The menu item of the entry.
 * @param {UmbActionMenuItem<T> | undefined} previous - The menu item before it, or undefined for the first item.
 * @returns {boolean} Whether a separator renders before the entry.
 */
function hasSeparatorBefore<T extends UmbGroupableAction>(
	entry: UmbActionMenuEntry<T>,
	isFirstOfItem: boolean,
	item: UmbActionMenuItem<T>,
	previous: UmbActionMenuItem<T> | undefined,
): boolean {
	const requested = entry.action.manifest?.separatorBefore === true;
	if (!isFirstOfItem) return requested;
	if (!previous) return false;
	return requested || item.group !== undefined || previous.group !== undefined;
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
