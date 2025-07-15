import type { JsLoaderProperty } from '../types/utils.js';

/**
 *
 * @param property
 */
export async function loadManifestPlainJs<JsType>(property: JsLoaderProperty<JsType>): Promise<JsType | undefined> {
	const propType = typeof property;
	if (propType === 'function') {
		// Promise function
		const result = await (property as Exclude<typeof property, string>)();
		if (typeof result === 'object' && result != null) {
			return result;
		}
	} else if (propType === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property as string) as unknown as JsType);
		if (typeof result === 'object' && result != null) {
			return result;
		}
	}
	return undefined;
}
