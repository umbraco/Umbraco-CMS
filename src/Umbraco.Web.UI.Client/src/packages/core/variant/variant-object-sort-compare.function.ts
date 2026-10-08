type UmbSortableVariantObject = { culture?: string | null; segment?: string | null };

/**
 * Compares two culture or segment codes, invariant (nullish) first, then ordinally and case-sensitively,
 * matching .NET's `StringComparer.Ordinal`.
 * @param {string | null | undefined} a - The first code to compare.
 * @param {string | null | undefined} b - The second code to compare.
 * @returns {number} A negative number if `a` sorts before `b`, positive if after, zero if equal.
 */
export function umbVariantCodeSortCompare(a: string | null | undefined, b: string | null | undefined): number {
	const normalizedA = a ?? null;
	const normalizedB = b ?? null;
	if (normalizedA === normalizedB) return 0;
	if (normalizedA === null) return -1;
	if (normalizedB === null) return 1;
	return normalizedA < normalizedB ? -1 : 1;
}

/**
 * Compares two variant objects by culture, then by segment, in the order the Management API returns
 * variant collections in (invariant first, then ordinal and case-sensitive).
 * This is not the display order of `sortVariants`, which prioritizes default language, mandatory state and publish state.
 * @param {UmbSortableVariantObject} a - The first object to compare.
 * @param {UmbSortableVariantObject} b - The second object to compare.
 * @returns {number} A negative number if `a` sorts before `b`, positive if after, zero if equal.
 */
export function umbVariantObjectSortCompare(a: UmbSortableVariantObject, b: UmbSortableVariantObject): number {
	return umbVariantCodeSortCompare(a.culture, b.culture) || umbVariantCodeSortCompare(a.segment, b.segment);
}
