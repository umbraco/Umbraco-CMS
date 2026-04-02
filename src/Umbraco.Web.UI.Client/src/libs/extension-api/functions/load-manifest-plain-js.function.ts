import type { JsLoaderPromise } from '../types/utils.js';

/**
 *
 * @param property
 */
export async function loadManifestPlainJs<JsType>(
	property: string | JsLoaderPromise<JsType> | JsType,
): Promise<JsType | undefined> {
	if (typeof property === 'function') {
		// Promise function (dynamic import)
		const result = await (property as JsLoaderPromise<JsType>)();
		if (typeof result === 'object' && result != null) {
			return result;
		}
	} else if (typeof property === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property) as unknown as JsType);
		if (typeof result === 'object' && result != null) {
			return result;
		}
	} else if (typeof property === 'object' && property != null) {
		// Already resolved module object (statically imported)
		return property;
	}
	return undefined;
}
