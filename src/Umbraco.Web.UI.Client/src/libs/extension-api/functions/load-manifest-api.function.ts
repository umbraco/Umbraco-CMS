import type { UmbApi } from '../models/api.interface.js';
import type {
	ApiLoaderExports,
	ApiLoaderProperty,
	ClassConstructor,
	ElementAndApiLoaderProperty,
	ElementLoaderExports,
} from '../types/utils.js';

/**
 *
 * @param property
 */
export async function loadManifestApi<ApiType extends UmbApi>(
	property: ApiLoaderProperty<ApiType> | ElementAndApiLoaderProperty<any, ApiType>,
): Promise<ClassConstructor<ApiType> | undefined> {
	const propType = typeof property;
	if (propType === 'function') {
		if ((property as ClassConstructor).prototype) {
			// Class Constructor
			return property as ClassConstructor<ApiType>;
		} else {
			// Promise function
			const result = await (
				property as Exclude<Exclude<ApiLoaderProperty<ApiType>, string>, ClassConstructor<ApiType>>
			)();
			if (typeof result === 'object' && result != null) {
				const exportValue =
					('api' in result ? result.api : undefined) ||
					('default' in result ? (result as Exclude<typeof result, ElementLoaderExports>).default : undefined);
				if (exportValue && typeof exportValue === 'function') {
					return exportValue;
				}
			}
		}
	} else if (propType === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property as string) as unknown as ApiLoaderExports<ApiType>);
		if (result && typeof result === 'object') {
			const exportValue =
				('api' in result ? result.api : undefined) || ('default' in result ? result.default : undefined);
			if (exportValue && typeof exportValue === 'function') {
				return exportValue;
			}
		}
	}
	return undefined;
}
