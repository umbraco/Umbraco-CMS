import type { ManifestElement, ManifestBase } from '../types';
import { isManifestElementNameType } from './is-manifest-element-name-type.function';
import { isManifestJSType } from './is-manifest-js-type.function';
import { isManifestLoaderType } from './is-manifest-loader-type.function';

export function isManifestElementableType<ElementType extends HTMLElement = HTMLElement>(
	manifest: ManifestBase
): manifest is ManifestElement {
	return (
		isManifestElementNameType(manifest) ||
		isManifestLoaderType<ElementType>(manifest) ||
		isManifestJSType<ElementType>(manifest)
	);
}
