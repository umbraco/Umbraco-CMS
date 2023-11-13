import type { ClassConstructor, ElementLoaderExports, ElementLoaderProperty } from "../types/utils.js";


export async function loadManifestElementProperty<ElementType extends HTMLElement>(property: ElementLoaderProperty<ElementType>): Promise<ElementType | undefined> {
	const propType = typeof property
	if(propType === 'function') {
		if(typeof property.constructor === 'function') {
			// Class Constructor
			return new (property as ClassConstructor<ElementType>)();
		} else {
			// Promise function
			const result = await (property as (Exclude<Exclude<typeof property, string>, ClassConstructor<ElementType>>))();
			if(typeof result === 'object' && result !== null) {
				const exportValue = 'element' in result ? result.element : undefined || 'default' in result ? result.default : undefined;
				if(exportValue && typeof exportValue === 'function') {
					return new exportValue();
				}
			}
		}
	} else if(propType === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property as string) as unknown as ElementLoaderExports<ElementType>);
		if(typeof result === 'object' && result != null) {
			const exportValue = 'element' in result ? result.element : undefined || 'default' in result ? result.default : undefined;
			if(exportValue && typeof exportValue === 'function') {
				return new exportValue();
			}
		}
	}
	return undefined;
}
