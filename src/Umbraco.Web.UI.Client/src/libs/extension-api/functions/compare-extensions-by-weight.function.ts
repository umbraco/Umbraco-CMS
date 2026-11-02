export type UmbWeightedExtension = { alias: string; weight?: number };

/**
 * Compares two extensions (or their controllers) by weight, highest first, then by alias, so extensions with equal
 * weights always get the same order.
 * @param {UmbWeightedExtension} a - The first extension.
 * @param {UmbWeightedExtension} b - The second extension.
 * @returns {number} A negative number when `a` sorts first, a positive number when `b` sorts first, otherwise 0.
 */
export function compareExtensionsByWeight(a: UmbWeightedExtension, b: UmbWeightedExtension): number {
	return (b.weight ?? 0) - (a.weight ?? 0) || (a.alias < b.alias ? -1 : a.alias > b.alias ? 1 : 0);
}
