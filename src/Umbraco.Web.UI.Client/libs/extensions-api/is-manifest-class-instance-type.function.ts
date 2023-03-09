import type { ManifestClass, ManifestClassWithClassConstructor } from '../models';

export function isManifestClassConstructorType(manifest: unknown): manifest is ManifestClassWithClassConstructor {
	return (
		typeof manifest === 'object' && manifest !== null && (manifest as ManifestClass).class !== undefined
	);
}
