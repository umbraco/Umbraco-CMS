import type { ManifestElement, ManifestBase } from '@umbraco-cms/extensions-registry';
import { isManifestElementNameType } from './is-manifest-element-name-type.function';
import { isManifestJSType } from './is-manifest-js-type.function';
import { isManifestLoaderType } from './is-manifest-loader-type.function';

export function isManifestElementableType(manifest: ManifestBase): manifest is ManifestElement {
	return isManifestElementNameType(manifest) || isManifestLoaderType(manifest) || isManifestJSType(manifest);
}
