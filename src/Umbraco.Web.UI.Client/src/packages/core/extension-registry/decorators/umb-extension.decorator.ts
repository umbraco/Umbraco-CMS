import { umbExtension as umbExtensionBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * A class decorator that stores extension manifest metadata on the class.
 *
 * This is the typed version that uses the full `UmbExtensionManifest`
 * union type, giving autocomplete and validation for all known extension types
 * (dashboards, entity actions, property editors, etc.).
 *
 * Loader properties (`element`, `api`, `js`, `elementName`) are accepted but ignored
 * at runtime — they are resolved from the module's exports instead.
 *
 * The decorator itself has no runtime side-effects — it only tags the class
 * with manifest data. Registration is handled by the bundle initializer or
 * by calling `registerExtensionModule` manually.
 * @param {UmbExtensionManifest} manifest - The extension manifest metadata.
 * @returns {ReturnType<typeof umbExtensionBase>} A class decorator function.
 */
export function umbExtension(manifest: UmbExtensionManifest) {
	return umbExtensionBase(manifest);
}
