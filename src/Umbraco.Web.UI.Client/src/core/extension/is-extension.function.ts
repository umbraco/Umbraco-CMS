import { UmbExtensionManifestBase } from './extension.registry';

export function isExtensionType(manifest: unknown): manifest is UmbExtensionManifestBase {
	return (
		typeof manifest === 'object' &&
		manifest !== null &&
		(manifest as UmbExtensionManifestBase).elementName !== undefined
	);
}
