import { ManifestElement, ManifestElementAndApi } from "../types/base.types.js";
import { loadManifestElement } from "./load-manifest-element.function.js";

export async function createExtensionElement<ElementType extends HTMLElement>(manifest: ManifestElement<ElementType> | ManifestElementAndApi<ElementType>, fallbackElement?: string): Promise<ElementType | undefined> {

	if(manifest.element) {
		const elementConstructor = await loadManifestElement<ElementType>(manifest.element);
		if(elementConstructor) {
			return new elementConstructor();
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an element class instance via the extension manifest property 'element', using either a 'element' or 'default' export`,
				manifest
			);
		}
	}

	if(manifest.js) {
		const elementConstructor2 = await loadManifestElement<ElementType>(manifest.js);
		if(elementConstructor2) {
			return new elementConstructor2();
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed creating an element class instance via the extension manifest property 'js', using either a 'element' or 'default' export`,
				manifest
			);
		}
	}

	if(manifest.elementName) {
		return document.createElement(manifest.elementName) as ElementType;
	}

	if(fallbackElement) {
		return document.createElement(fallbackElement) as ElementType;
	}

	console.error(
		`-- Extension of alias "${manifest.alias}" did not succeed creating an element, missing a JavaScript file via the 'element' or 'js' property or a Element Name in 'elementName' in the manifest.`,
		manifest
	);
	return undefined;
}
