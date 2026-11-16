/**
 * An extension manifest, or a controller of one, that can be ordered by `compareExtensionsByWeight`.
 */
export type UmbWeightedExtension = { alias: string; weight?: number };

/**
 * Compares two extensions (or their controllers) by weight, highest first, then by alias, so extensions with equal
 * weights always get the same order.
 * @param {UmbWeightedExtension} a - The first extension.
 * @param {UmbWeightedExtension} b - The second extension.
 * @returns {number} A negative number when `a` sorts first, a positive number when `b` sorts first, otherwise 0.
 */
export function compareExtensionsByWeight(a: UmbWeightedExtension, b: UmbWeightedExtension): number {
	const byWeight = (b.weight ?? 0) - (a.weight ?? 0);
	if (byWeight !== 0) return byWeight;
	if (a.alias === b.alias) return 0;
	return a.alias < b.alias ? -1 : 1;
}
