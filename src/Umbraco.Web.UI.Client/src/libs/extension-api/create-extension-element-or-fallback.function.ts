import { createExtensionElement } from './create-extension-element.function.js';
import { isManifestElementableType } from './type-guards/index.js';

export async function createExtensionElementOrFallback(
	manifest: any,
	fallbackElementName: string
): Promise<HTMLElement | undefined> {
	if (isManifestElementableType(manifest)) {
		return createExtensionElement(manifest);
	}

	return Promise.resolve(document.createElement(fallbackElementName));
}
