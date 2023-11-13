import type { UmbApi } from "../models/api.interface.js";
import type { ApiLoaderExports, ApiLoaderProperty, ClassConstructor } from "../types/utils.js";

export async function loadManifestApiProperty<ApiType extends UmbApi>(property: ApiLoaderProperty<ApiType>, constructorArguments: unknown[] = []): Promise<ApiType | undefined> {
	const propType = typeof property
	if(propType === 'function') {
		if(typeof property.constructor === 'function') {
			// Class Constructor
			return new (property as ClassConstructor<ApiType>)(constructorArguments);
		} else {
			// Promise function
			const result = await (property as (Exclude<Exclude<ApiLoaderProperty<ApiType>, string>, ClassConstructor<ApiType>>))();
			if(typeof result === 'object' && result != null) {
				const exportValue = 'api' in result ? result.api : undefined || 'default' in result ? result.default : undefined;
				if(exportValue && typeof exportValue === 'function') {
					return new exportValue(constructorArguments);
				}
			}
		}
	} else if(propType === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property as string) as unknown as ApiLoaderExports<ApiType>);
		if(typeof result === 'object' && result != null) {
			const exportValue = 'api' in result ? result.api : undefined || 'default' in result ? result.default : undefined;
			if(exportValue && typeof exportValue === 'function') {
				return new exportValue(constructorArguments);
			}
		}
	}
	return undefined;
}