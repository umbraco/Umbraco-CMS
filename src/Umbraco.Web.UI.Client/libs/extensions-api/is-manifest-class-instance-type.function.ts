import type { ManifestClass, ManifestClassWithClassConstructor } from '@umbraco-cms/backoffice/extensions-registry';

export function isManifestClassConstructorType(manifest: unknown): manifest is ManifestClassWithClassConstructor {
	return typeof manifest === 'object' && manifest !== null && (manifest as ManifestClass).class !== undefined;
}
