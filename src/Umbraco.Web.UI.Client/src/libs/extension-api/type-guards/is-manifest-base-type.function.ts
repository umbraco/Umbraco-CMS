import type { ManifestBase } from '../types/manifest-base.interface.js';

/**
 *
 * @param x
 */
export function isManifestBaseType(x: unknown): x is ManifestBase {
	return typeof x === 'object' && x !== null && 'alias' in x;
}
