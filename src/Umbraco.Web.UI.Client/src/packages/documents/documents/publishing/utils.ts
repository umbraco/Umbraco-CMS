import { UmbDocumentVariantState } from '../variant-state.js';
import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbDocumentAncestorPublishCoverageModel } from './schedule-publish/modal/document-schedule-modal.token.js';

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
 * Minimal shape required by {@link computeAncestorPublishCoverage}.
 * Picks out only the fields needed so the helper can be unit-tested without
 * binding to the full API model.
 */
export interface UmbAncestorVariantForCoverage {
	culture: string | null;
	state: UmbDocumentVariantState | null;
}
export interface UmbAncestorForCoverage {
	variants: ReadonlyArray<UmbAncestorVariantForCoverage>;
}

/**
 * Computes how well the supplied ancestor chain covers a document that is being published.
 *
 * A variant counts as published if its state is `Published` or `PublishedPendingChanges`.
 * Publishing requires every ancestor to be published in at least one culture, whereas a
 * culture is only visible when every ancestor is published in that culture. An ancestor
 * with its invariant variant published covers every culture.
 * @param {ReadonlyArray<UmbAncestorForCoverage>} ancestors The ancestors, in any order.
 * @returns {UmbDocumentAncestorPublishCoverageModel | undefined} The coverage, or `undefined` when there are no ancestors.
 */
export function computeAncestorPublishCoverage(
	ancestors: ReadonlyArray<UmbAncestorForCoverage>,
): UmbDocumentAncestorPublishCoverageModel | undefined {
	if (ancestors.length === 0) return undefined;

	let isPathPublished = true;
	let publishedCultures: Set<string> | null = null;

	for (const ancestor of ancestors) {
		const ancestorPublished = ancestor.variants
			.filter(
				(variant) =>
					variant.state === UmbDocumentVariantState.PUBLISHED ||
					variant.state === UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES,
			)
			.map((variant) => variant.culture);

		if (ancestorPublished.length === 0) isPathPublished = false;
		if (ancestorPublished.includes(null)) continue;

		const ancestorCultures = new Set(ancestorPublished.filter((culture): culture is string => culture !== null));
		publishedCultures =
			publishedCultures === null
				? ancestorCultures
				: new Set([...publishedCultures].filter((culture: string) => ancestorCultures.has(culture)));
	}

	return { isPathPublished, publishedCultures: publishedCultures === null ? null : Array.from(publishedCultures) };
}
