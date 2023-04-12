import type { ManifestElement, ManifestElementWithElementName } from '@umbraco-cms/backoffice/extensions-registry';

export function isManifestElementNameType(manifest: unknown): manifest is ManifestElementWithElementName {
	return typeof manifest === 'object' && manifest !== null && (manifest as ManifestElement).elementName !== undefined;
}
