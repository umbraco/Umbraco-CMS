import type { UmbEntityStateLook } from './entity-state-look.js';
import type { UmbState } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbEntityStateEntry extends UmbState {
	look?: UmbEntityStateLook;
	/**
	 * Extra detail shown as a tooltip on hover — rendered via the native `title` attribute for now, until a
	 * custom tooltip component exists to replace it. Keep `message` itself short and generic; put anything more
	 * specific here.
	 */
	detail?: string;
	/**
	 * Orders entries when several are active at once. Never excludes — a low-weight entry is never dropped
	 * in favor of a high-weight one, it's just rendered after it. Defaults to 0.
	 *
	 * Convention for new producers: reserve 100+ for entity-wide, blocking/destructive states (e.g. trashed),
	 * 1-99 for descriptive lifecycle states (e.g. publish-state), 0 for baseline/fallback informational states.
	 */
	weight?: number;
}

export class UmbEntityStateManager<
	EntryType extends UmbEntityStateEntry = UmbEntityStateEntry,
> extends UmbControllerBase {
	protected readonly _states = new UmbArrayState<EntryType>([], (x) => x.unique);

	/** All states, weight-sorted (highest first). Never excludes — filtering (e.g. by variantId) is the resolving subclass's job. */
	public readonly states = this._states.asObservablePart((states) => this.#sortByWeight(states));

	#sortByWeight(states: Array<EntryType>): Array<EntryType> {
		return [...states].sort((a, b) => (b.weight ?? 0) - (a.weight ?? 0));
	}

	/**
	 * Add a new state. Upserts by `unique` — unlike `UmbStateManager.addState()`, this never throws on a repeat key.
	 * @param {EntryType} state - The state to add.
	 */
	addState(state: EntryType) {
		this._states.appendOne(state);
	}

	/**
	 * Add multiple states.
	 * @param {Array<EntryType>} states - The states to add.
	 */
	addStates(states: Array<EntryType>) {
		this._states.mute();
		states.forEach((s) => this.addState(s));
		this._states.unmute();
	}

	/**
	 * Remove a state.
	 * @param {EntryType['unique']} unique - Unique value of the state to remove.
	 */
	removeState(unique: EntryType['unique']) {
		this._states.removeOne(unique);
	}

	/**
	 * Remove multiple states.
	 * @param {Array<EntryType['unique']>} uniques - Array of unique values to remove.
	 */
	removeStates(uniques: Array<EntryType['unique']>) {
		this._states.remove(uniques);
	}

	/**
	 * Get all states, weight-sorted (highest first).
	 * @returns {Array<EntryType>} Weight-sorted array of states.
	 */
	getStates(): Array<EntryType> {
		return this.#sortByWeight(this._states.getValue());
	}

	/**
	 * Replaces every state matching `predicate` with `newStates` in one update. For a producer that recomputes a
	 * variable-length state set on every change (e.g. one state per variant) instead of toggling a fixed set of
	 * keys — this owns the diff-and-remove-stale bookkeeping so producers don't each reinvent it.
	 * @param {(state: EntryType) => boolean} predicate - Matches the states to remove before `newStates` are added.
	 * @param {Array<EntryType>} newStates - The states to add in place of the removed ones.
	 */
	replaceStates(predicate: (state: EntryType) => boolean, newStates: Array<EntryType>): void {
		this._states.setValue([...this._states.getValue().filter((s) => !predicate(s)), ...newStates]);
	}

	/** Clear all states. */
	clear(): void {
		this._states.setValue([]);
	}

	override destroy(): void {
		this._states.destroy();
		super.destroy();
	}
}
