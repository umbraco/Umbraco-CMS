import { ManifestTypes } from '../models';
import { hasDefaultExport } from './has-default-export.function';
import { isManifestElementType } from './is-extension.function';
import { loadExtension } from './load-extension.function';

export async function createExtensionElement(manifest: ManifestTypes): Promise<HTMLElement | undefined> {
	//TODO: Write tests for these extension options:
	const js = await loadExtension(manifest);
	if (isManifestElementType(manifest) && manifest.elementName) {
		// created by manifest method providing HTMLElement
		return document.createElement(manifest.elementName);
	}
	if (js) {
		if (js instanceof HTMLElement) {
			console.log('-- created by manifest method providing HTMLElement', js);
			return js;
		}
		if (isManifestElementType(js) && js.elementName) {
			// created by js export elementName
			return document.createElement(js.elementName);
		}

		if (hasDefaultExport(js)) {
			// created by default class
			return new js.default();
		}
	}
	console.error('-- Extension did not succeed creating an element');
	return Promise.resolve(undefined);
}
