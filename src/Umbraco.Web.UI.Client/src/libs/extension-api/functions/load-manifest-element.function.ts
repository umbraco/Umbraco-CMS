import type {
	ApiLoaderExports,
	ClassConstructor,
	ElementAndApiLoaderProperty,
	ElementLoaderExports,
	ElementLoaderProperty,
} from '../types/utils.js';

/**
 *
 * @param property
 */
export async function loadManifestElement<ElementType extends HTMLElement>(
	property: ElementLoaderProperty<ElementType> | ElementAndApiLoaderProperty<ElementType>,
): Promise<ClassConstructor<ElementType> | undefined> {
	const propType = typeof property;
	if (propType === 'function') {
		if ((property as ClassConstructor).prototype) {
			// Class Constructor
			return property as ClassConstructor<ElementType>;
		} else {
			// Promise function
			const result = await (property as Exclude<Exclude<typeof property, string>, ClassConstructor<ElementType>>)();
			if (typeof result === 'object' && result !== null) {
				const exportValue =
					('element' in result ? result.element : undefined) ||
					('default' in result ? (result as Exclude<typeof result, ApiLoaderExports>).default : undefined);
				if (exportValue && typeof exportValue === 'function') {
					return exportValue;
				}
			}
		}
	} else if (propType === 'string') {
		// Import string
		const result = await (import(
			/* @vite-ignore */ property as string
		) as unknown as ElementLoaderExports<ElementType>);
		if (result && typeof result === 'object') {
			const exportValue =
				('element' in result ? result.element : undefined) || ('default' in result ? result.default : undefined);
			if (exportValue && typeof exportValue === 'function') {
				return exportValue;
			}
		}
	}
	return undefined;
}
