import type { ManifestActionGroupBase } from './action-group.extension.js';

/**
 * An action (or its extension controller) that can be ordered by `umbOrderActionsByGroup`.
 */
export interface UmbGroupableAction {
	alias: string;
	weight?: number;
	manifest?: { group?: string; separatorBefore?: boolean };
}

/**
 * A block of an ordered action menu: the actions of one registered group, or the actions without a registered group.
 */
export interface UmbActionGroupBlock<T extends UmbGroupableAction> {
	/** The registered group, or undefined for the block of actions without a registered group. */
	group?: ManifestActionGroupBase;
	/** The actions of the block, ordered by weight, then by alias. */
	actions: Array<T>;
}

/**
 * Orders the visible actions of one menu into blocks: one per registered group with visible actions, ordered by the
 * group's weight, then one block with the actions that have no registered group. Inside a block, actions are ordered
 * by their own weight. Equal weights are ordered by alias.
 * @template T - The type of the actions.
 * @param {ReadonlyArray<T>} actions - The visible actions of the menu.
 * @param {ReadonlyArray<ManifestActionGroupBase>} groups - The registered groups of the menu's action type.
 * @returns {Array<UmbActionGroupBlock<T>>} The blocks, in render order.
 */
export function umbOrderActionsByGroup<T extends UmbGroupableAction>(
	actions: ReadonlyArray<T>,
	groups: ReadonlyArray<ManifestActionGroupBase>,
): Array<UmbActionGroupBlock<T>> {
	const groupsByAlias = new Map(groups.map((group) => [group.alias, group]));
	const actionsByGroup = Map.groupBy([...actions].sort(byWeightThenAlias), (action) =>
		action.manifest?.group ? groupsByAlias.get(action.manifest.group) : undefined,
	);
	return Array.from(actionsByGroup, ([group, actions]) => ({ group, actions })).sort((a, b) =>
		a.group && b.group ? byWeightThenAlias(a.group, b.group) : Number(!a.group) - Number(!b.group),
	);
}

const byWeightThenAlias = (a: { alias: string; weight?: number }, b: { alias: string; weight?: number }) =>
	(b.weight ?? 0) - (a.weight ?? 0) || a.alias.localeCompare(b.alias);
