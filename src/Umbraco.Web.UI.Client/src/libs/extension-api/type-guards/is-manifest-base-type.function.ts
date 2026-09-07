import type { ManifestBase } from '../types/manifest-base.interface.js';

/**
 * Validate if an object is a ManifestBase.
 * @param {unknown} x The object to check.
 * @returns {boolean} True if the object has an 'alias' property.
 */
export function isManifestBaseType(x: unknown): x is ManifestBase {
	return typeof x === 'object' && x !== null && 'alias' in x;
}
