import type { ManifestElement, ManifestElementWithElementName } from '../types/index.js';

/**
 * Type guard to check if a manifest has an `elementName` property.
 * @param {unknown} manifest - the manifest to check.
 * @returns {boolean} - true if the manifest has an `elementName` property.
 */
export function isManifestElementNameType(manifest: unknown): manifest is ManifestElementWithElementName {
	return typeof manifest === 'object' && manifest !== null && (manifest as ManifestElement).elementName !== undefined;
}
