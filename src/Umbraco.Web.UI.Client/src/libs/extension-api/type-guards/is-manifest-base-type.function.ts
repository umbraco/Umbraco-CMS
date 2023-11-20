import type { ManifestBase } from "../types/manifest-base.interface.js";

export function isManifestBaseType(x: unknown): x is ManifestBase {
	return typeof x === 'object' && x !== null && 'alias' in x;
}