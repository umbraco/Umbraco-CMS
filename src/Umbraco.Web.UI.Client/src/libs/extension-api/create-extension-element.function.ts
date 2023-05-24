import { hasDefaultExport } from './has-default-export.function.js';
import { isManifestElementNameType } from './type-guards/index.js';
import { loadExtension } from './load-extension.function.js';
import type { HTMLElementConstructor, ManifestElement } from './types.js';

export async function createExtensionElement<ElementType extends HTMLElement>(
	manifest: ManifestElement<ElementType>
): Promise<ElementType | undefined> {
	//TODO: Write tests for these extension options:
	const js = await loadExtension(manifest);

	if (isManifestElementNameType(manifest)) {
		// created by manifest method providing HTMLElement
		return document.createElement(manifest.elementName) as ElementType;
	}

	// TODO: Do we need this except for the default() loader?
	if (js) {
		if (hasDefaultExport<HTMLElementConstructor<ElementType>>(js)) {
			// Element will be created by default class
			return new js.default();
		}

		// If some JS was loaded and manifest did not have a elementName neither it the JS file contain a default export, so we will fail:
		console.error(
			'-- Extension did not succeed creating an element, missing a default export of the served JavaScript file',
			manifest
		);
		return undefined;
	}

	// If some JS was loaded and manifest did not have a elementName neither it the JS file contain a default export, so we will fail:
	console.error(
		'-- Extension did not succeed creating an element, missing a default export or `elementName` in the manifest.',
		manifest
	);
	return undefined;
}
