import { UmbDocumentVariantState } from '../variant-state.js';
import type { UmbDocumentVariantOptionModel } from '../types.js';

/**
 * @function isNotPublishedMandatory
 * @param {UmbDocumentVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isNotPublishedMandatory(option: UmbDocumentVariantOptionModel): boolean {
	return (
		option.language.isMandatory &&
		option.variant?.state !== UmbDocumentVariantState.PUBLISHED &&
		option.variant?.state !== UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES
	);
}

/**
 * Minimal shape required by {@link computeAncestorPublishedCultures}.
 * Picks out only the fields needed for the intersection so the helper can be
 * unit-tested without binding to the full API model.
 */
export interface UmbAncestorVariantForCoverage {
	culture: string | null;
	state?: UmbDocumentVariantState | string | null;
}
export interface UmbAncestorForCoverage {
	variants: ReadonlyArray<UmbAncestorVariantForCoverage>;
}

/**
 * Computes which cultures are published across every ancestor in the supplied chain.
 *
 * For a scheduled publish to take effect on a child variant, every ancestor must be
 * published in that culture. A variant counts as "published" if its state is
 * `Published` or `PublishedPendingChanges`. An ancestor with the invariant variant
 * published (culture === null) covers every child culture and adds no constraint.
 * @param ancestors The ordered list of ancestors (any order works — the result is an intersection).
 * @returns
 *  - `undefined` when there are no ancestors (root document) — the caller renders no warning;
 *  - `[null]` when no ancestor adds a constraint (every ancestor is invariant-published) — covers all child cultures;
 *  - otherwise the cultures published in every ancestor.
 */
export function computeAncestorPublishedCultures(
	ancestors: ReadonlyArray<UmbAncestorForCoverage>,
): Array<string | null> | undefined {
	if (ancestors.length === 0) return undefined;

	let covered: Set<string | null> | undefined;

	for (const ancestor of ancestors) {
		const ancestorPublished: Array<string | null> = ancestor.variants
			.filter(
				(variant) =>
					variant.state === UmbDocumentVariantState.PUBLISHED ||
					variant.state === UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES,
			)
			.map((variant) => variant.culture);

		// An invariant-published ancestor (null entry) covers every child culture — no constraint added.
		if (ancestorPublished.includes(null)) continue;

		const ancestorSet = new Set<string | null>(ancestorPublished);
		if (covered === undefined) {
			covered = ancestorSet;
		} else {
			const intersection = new Set<string | null>();
			for (const culture of covered) {
				if (ancestorSet.has(culture)) intersection.add(culture);
			}
			covered = intersection;
		}
	}

	return covered === undefined ? [null] : Array.from(covered);
}
