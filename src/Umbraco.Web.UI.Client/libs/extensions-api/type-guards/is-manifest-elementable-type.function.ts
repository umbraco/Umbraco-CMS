import { isManifestElementNameType } from './is-manifest-element-name-type.function';
import { isManifestJSType } from './is-manifest-js-type.function';
import { isManifestLoaderType } from './is-manifest-loader-type.function';
import type { ManifestElement, ManifestBase } from '@umbraco-cms/backoffice/extensions-registry';

export function isManifestElementableType<ElementType extends HTMLElement = HTMLElement>(
	manifest: ManifestBase
): manifest is ManifestElement {
	return (
		isManifestElementNameType(manifest) ||
		isManifestLoaderType<ElementType>(manifest) ||
		isManifestJSType<ElementType>(manifest)
	);
}
