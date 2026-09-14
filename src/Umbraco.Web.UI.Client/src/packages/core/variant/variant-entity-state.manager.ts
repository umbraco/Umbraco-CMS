import type { UmbVariantId } from './variant-id.class.js';
import { UmbEntityStateManager, type UmbEntityStateEntry } from '@umbraco-cms/backoffice/entity-state';
import { createObservablePart, mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbVariantEntityStateEntry extends UmbEntityStateEntry {
	/** Omit to apply to every variant. Set to target one specific variant. */
	variantId?: UmbVariantId;
}

/**
 * `get*` methods are synchronous snapshots; the bare names are their observable, live-updating counterparts —
 * same convention as the base class's `getStates()`/`states` pair.
 */
export class UmbVariantEntityStateManager extends UmbEntityStateManager<UmbVariantEntityStateEntry> {
	/**
	 * Get all states that apply to this variant (universal + variant-specific), highest weight first.
	 * @param {UmbVariantId} variantId - The variantId to resolve states for.
	 * @returns {Array<UmbVariantEntityStateEntry>} Weight-sorted array of matching states.
	 */
	getStatesForVariant(variantId: UmbVariantId): Array<UmbVariantEntityStateEntry> {
		return this.getStates().filter((s) => s.variantId === undefined || s.variantId.compare(variantId));
	}

	/**
	 * Observe live-updating states for this variant — re-emits whenever the registry changes.
	 * @param {UmbVariantId} variantId - The variantId to resolve states for.
	 * @returns {Observable<Array<UmbVariantEntityStateEntry>>} Observable emitting the weight-sorted array of matching states.
	 */
	statesForVariant(variantId: UmbVariantId): Observable<Array<UmbVariantEntityStateEntry>> {
		return createObservablePart(this.states, (states) =>
			states.filter((s) => s.variantId === undefined || s.variantId.compare(variantId)),
		);
	}

	/**
	 * Get all states that apply to each of the given variants, highest weight first.
	 * @param {Array<UmbVariantId>} variantIds - The variantIds to resolve states for.
	 * @returns {Array<{ variantId: UmbVariantId; states: Array<UmbVariantEntityStateEntry> }>} Each variantId paired with its matching states.
	 */
	getStatesForVariants(
		variantIds: Array<UmbVariantId>,
	): Array<{ variantId: UmbVariantId; states: Array<UmbVariantEntityStateEntry> }> {
		return variantIds.map((variantId) => ({ variantId, states: this.getStatesForVariant(variantId) }));
	}

	/**
	 * Observe live-updating states for a set of variants — re-emits whenever the registry or the variant list changes.
	 * @param {Observable<Array<UmbVariantId>>} variantIds - Observable emitting the variantIds to resolve states for.
	 * @returns {Observable<Array<{ variantId: UmbVariantId; states: Array<UmbVariantEntityStateEntry> }>>} Observable emitting each variantId paired with its matching states.
	 */
	statesForVariants(
		variantIds: Observable<Array<UmbVariantId>>,
	): Observable<Array<{ variantId: UmbVariantId; states: Array<UmbVariantEntityStateEntry> }>> {
		return mergeObservables([this.states, variantIds], ([, ids]) => this.getStatesForVariants(ids ?? []));
	}
}
