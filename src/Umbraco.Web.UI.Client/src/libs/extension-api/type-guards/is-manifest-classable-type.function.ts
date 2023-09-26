import type { ManifestBase, ManifestApi } from '../types.js';
import { isManifestJSType } from './is-manifest-js-type.function.js';
import { isManifestLoaderType } from './is-manifest-loader-type.function.js';
import { isManifestClassConstructorType } from './is-manifest-class-instance-type.function.js';

export function isManifestClassableType(manifest: ManifestBase): manifest is ManifestApi {
	return (
		isManifestClassConstructorType(manifest) ||
		isManifestLoaderType<object>(manifest) ||
		isManifestJSType<object>(manifest)
	);
}
