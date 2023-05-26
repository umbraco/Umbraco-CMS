import type { ManifestElement, ManifestBase } from '../types.js';
import { isManifestElementNameType } from './is-manifest-element-name-type.function.js';
import { isManifestJSType } from './is-manifest-js-type.function.js';
import { isManifestLoaderType } from './is-manifest-loader-type.function.js';

export function isManifestElementableType<ElementType extends HTMLElement = HTMLElement>(
	manifest: ManifestBase
): manifest is ManifestElement {
	return (
		isManifestElementNameType(manifest) ||
		isManifestLoaderType<ElementType>(manifest) ||
		isManifestJSType<ElementType>(manifest)
	);
}
