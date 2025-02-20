import type { UmbApi } from '../models/api.interface.js';
import type { ManifestElementAndApi } from '../types/base.types.js';
import type { ApiLoaderProperty, ClassConstructor } from '../types/utils.js';
import { loadManifestApi } from './load-manifest-api.function.js';
import { loadManifestElement } from './load-manifest-element.function.js';
import type { UmbApiConstructorArgumentsMethodType } from './types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 *
 * @param manifest
 * @param constructorArgs
 * @param fallbackElement
 * @param fallbackApi
 * @returns {Promise<{ element?: ElementType; api?: ApiType }>} - Returns an object with the created element and api.
 */
export async function createExtensionElementWithApi<
	ElementType extends UmbControllerHostElement,
	ApiType extends UmbApi = UmbApi,
>(
	manifest: ManifestElementAndApi<ElementType, ApiType>,
	constructorArgs?: unknown[] | UmbApiConstructorArgumentsMethodType<ManifestElementAndApi<ElementType, ApiType>>,
	fallbackElement?: string,
	fallbackApi?: ApiLoaderProperty<ApiType>,
): Promise<{ element?: ElementType; api?: ApiType }> {
	const apiPropValue = manifest.api ?? manifest.js ?? fallbackApi;
	if (!apiPropValue) {
		console.error(
			`-- Extension of alias "${manifest.alias}" did not succeed creating an api class instance, missing a JavaScript file via the 'api' or 'js' property, using either a 'api' or 'default'(not supported on the JS property) export`,
			manifest,
		);
		return {};
	}
	const apiPromise = loadManifestApi<ApiType>(apiPropValue);
	let apiConstructor: ClassConstructor<ApiType> | undefined;

	let element;

	const elementPropValue = manifest.element ?? manifest.js;
	if (elementPropValue) {
		const elementPromise = loadManifestElement<ElementType>(elementPropValue);
		const promises = await Promise.all([apiPromise, elementPromise]);
		apiConstructor = promises[0];
		const elementConstructor = promises[1];
		if (elementConstructor) {
			element = new elementConstructor();
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an element class instance via the extension manifest property 'element' or 'js', using either a 'element' or 'default'(not supported on the JS property) export`,
				manifest,
			);
		}
	} else {
		apiConstructor = await apiPromise;
	}

	if (!element && manifest.elementName) {
		element = document.createElement(manifest.elementName) as ElementType;
	}
	if (!element && fallbackElement) {
		element = document.createElement(fallbackElement) as ElementType;
	}

	if (element && apiConstructor) {
		// If constructorArgs is a function, call it with the manifest to get the arguments:
		const additionalArgs = (typeof constructorArgs === 'function' ? constructorArgs(manifest) : constructorArgs) ?? [];

		const api = new apiConstructor(element, ...additionalArgs);
		// Return object with element & api:
		return { element, api };
	}

	// Debug messages:
	if (!element && apiConstructor) {
		// If we have a elementPropValue, that means that element or js property was defined, but the element was not created.
		if (elementPropValue) {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an Element with Api, Api was created but the imported Element JS file did not export a 'element' or 'default'. Alternatively define the 'elementName' in the manifest.`,
				manifest,
			);
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an Element with Api, Api was created but the Element was missing a JavaScript file via the 'element' or the 'js' property. Alternatively define a Element Name in 'elementName' in the manifest.`,
				manifest,
			);
		}
	} else if (element && !apiConstructor) {
		if (apiPropValue) {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an Element with Api, Element was created but the imported Api JS file did not export a 'api' or 'default'.`,
				manifest,
			);
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an Element with Api, Element was created but the Api is missing a JavaScript file via the 'api' or 'js' property.`,
				manifest,
			);
		}
	} else {
		console.error(
			`-- Extension of alias "${manifest.alias}" did not succeed creating an Element with Api, neither an Element or Api was created, missing one or two JavaScript files via the 'element' and 'api' or the 'js' property or with just 'api' and the Element Name in 'elementName' in the manifest.`,
			manifest,
		);
	}
	return {};
}
