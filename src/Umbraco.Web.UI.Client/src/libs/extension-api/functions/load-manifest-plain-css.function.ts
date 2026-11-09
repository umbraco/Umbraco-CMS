import type { CssLoaderExports, CssLoaderProperty } from '../types/utils.js';

/**
 * Loads plain CSS from a manifest's `css` property.
 * @template {string} CssType
 * @param {CssLoaderProperty<CssType>} property The manifest property to load the CSS from.
 * @returns {Promise<CssType | undefined>} The resolved CSS string, if found.
 */
export async function loadManifestPlainCss<CssType extends string>(
	property: CssLoaderProperty<CssType>,
): Promise<CssType | undefined> {
	const propType = typeof property;
	if (propType === 'function') {
		// Promise function
		const result = await (property as Exclude<typeof property, string>)();
		if (typeof result === 'object' && result != null) {
			const exportValue =
				('css' in result ? result.css : undefined) || ('default' in result ? result.default : undefined);
			if (exportValue && typeof exportValue === 'string') {
				return exportValue as CssType;
			}
		}
	} else if (propType === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property as string) as unknown as CssLoaderExports<CssType>);
		if (typeof result === 'object' && result != null) {
			const exportValue =
				('css' in result ? result.css : undefined) || ('default' in result ? result.default : undefined);
			if (exportValue && typeof exportValue === 'string') {
				return exportValue;
			}
		}
	}
	return undefined;
}
