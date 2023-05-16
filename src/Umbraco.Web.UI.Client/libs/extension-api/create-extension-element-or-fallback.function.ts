import { createExtensionElement } from './create-extension-element.function';
import { isManifestElementableType } from './type-guards';

export async function createExtensionElementOrFallback(
	manifest: any,
	fallbackElementName: string
): Promise<HTMLElement | undefined> {
	if (isManifestElementableType(manifest)) {
		return createExtensionElement(manifest);
	}

	return Promise.resolve(document.createElement(fallbackElementName));
}
