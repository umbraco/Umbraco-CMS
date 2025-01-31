import type { CssLoaderExports, CssLoaderProperty } from '../types/utils.js';

/**
 *
 * @param property
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
