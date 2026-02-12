import type {
	ApiLoaderExports,
	ClassConstructor,
	ElementAndApiLoaderProperty,
	ElementLoaderExports,
	ElementLoaderPromise,
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
			// Promise function (dynamic import)
			const result = await (property as ElementLoaderPromise<ElementType>)();
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
	} else if (propType === 'object' && property !== null) {
		// Already resolved module object (statically imported)
		const result = property as ElementLoaderExports<ElementType>;
		const exportValue =
			('element' in result ? result.element : undefined) || ('default' in result ? result.default : undefined);
		if (exportValue && typeof exportValue === 'function') {
			return exportValue;
		}
	}
	return undefined;
}
