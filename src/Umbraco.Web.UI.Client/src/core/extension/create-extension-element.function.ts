import { ManifestCore } from '../models';
import { hasDefaultExport } from './has-default-export.function';
import { isManifestElementType } from './is-extension.function';
import { loadExtension } from './load-extension.function';

export async function createExtensionElement(manifest: ManifestCore): Promise<HTMLElement | undefined> {
	console.log('🚀 ~ file: create-extension-element.function.ts ~ line 7 ~ createExtensionElement ~ manifest', manifest);
	//TODO: Write tests for these extension options:
	const js = await loadExtension(manifest);
	console.log('🚀 ~ file: create-extension-element.function.ts ~ line 9 ~ createExtensionElement ~ js', js);
	if (isManifestElementType(manifest) && manifest.elementName) {
		// created by manifest method providing HTMLElement
		return document.createElement(manifest.elementName);
	}
	if (js) {
		if (js instanceof HTMLElement) {
			console.log('-- created by manifest method providing HTMLElement', js);
			return js;
		}
		if ('elementName' in js) {
			// created by js export elementName
			return document.createElement((js as any).elementName);
		}

		if (hasDefaultExport(js)) {
			// created by default class
			return new js.default();
		}
	}
	console.error('-- Extension did not succeed creating an element');
	return Promise.resolve(undefined);
}
