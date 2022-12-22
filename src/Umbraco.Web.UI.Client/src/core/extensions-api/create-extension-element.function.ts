import type { ManifestElementType } from '../models';
import { hasDefaultExport } from './has-default-export.function';
import { isManifestElementNameType } from './is-manifest-element-name-type.function';
import { loadExtension } from './load-extension.function';

export async function createExtensionElement(manifest: ManifestElementType): Promise<HTMLElement | undefined> {
	
	//TODO: Write tests for these extension options:
	const js = await loadExtension(manifest);

	if (isManifestElementNameType(manifest)) {
		// created by manifest method providing HTMLElement
		return document.createElement(manifest.elementName);
	}

	// TODO: Do we need this except for the default() loader?
	if (js) {
		if (hasDefaultExport(js)) {
			// created by default class
			return new js.default();
		}

		console.error('-- Extension did not succeed creating an element, missing a default export of the served JavaScript file', manifest);

		// If some JS was loaded and it did not at least contain a default export, then we are safe to assume that it executed as a module and does not need to be returned
		return undefined;
	}

	console.error('-- Extension did not succeed creating an element, missing a default export or `elementName` in the manifest.', manifest);
	return undefined;
}
