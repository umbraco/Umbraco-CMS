import { hasDefaultExport } from './has-default-export.function';
import { isManifestElementNameType } from './is-manifest-element-name-type.function';
import { loadExtension } from './load-extension.function';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/models';
import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-registry';
import type { PageComponent } from '@umbraco-cms/backoffice/router';

export async function createExtensionElement(manifest: ManifestElement): Promise<PageComponent> {
	//TODO: Write tests for these extension options:
	const js = await loadExtension(manifest);

	if (isManifestElementNameType(manifest)) {
		// created by manifest method providing HTMLElement
		return document.createElement(manifest.elementName);
	}

	// TODO: Do we need this except for the default() loader?
	if (js) {
		if (hasDefaultExport<HTMLElementConstructor>(js)) {
			// created by default class
			return new js.default();
		}

		console.error(
			'-- Extension did not succeed creating an element, missing a default export of the served JavaScript file',
			manifest
		);

		// If some JS was loaded and it did not at least contain a default export, then we are safe to assume that it executed its side effects and does not need to be returned
		return undefined;
	}

	console.error(
		'-- Extension did not succeed creating an element, missing a default export or `elementName` in the manifest.',
		manifest
	);
	return undefined;
}
