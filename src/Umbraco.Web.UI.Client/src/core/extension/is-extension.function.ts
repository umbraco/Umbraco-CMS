import type { ManifestElementType } from '../models';

export function isManifestElementType(manifest: unknown): manifest is ManifestElementType {
	return (
		typeof manifest === 'object' && manifest !== null && (manifest as ManifestElementType).elementName !== undefined
	);
}
