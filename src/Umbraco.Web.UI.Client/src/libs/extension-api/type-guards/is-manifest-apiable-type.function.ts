import type { ManifestBase, ManifestApi } from '../types.js';
import { isManifestJSType } from './is-manifest-js-type.function.js';
import { isManifestLoaderType } from './is-manifest-loader-type.function.js';
import { isManifestApiConstructorType } from './is-manifest-api-instance-type.function.js';
import { isManifestApiJSType } from './is-manifest-api-js-type.function.js';

export function isManifestApiType(manifest: ManifestBase): manifest is ManifestApi {
	return (
		isManifestApiConstructorType(manifest) ||
		isManifestLoaderType<object>(manifest) ||
		isManifestApiJSType<object>(manifest) ||
		isManifestJSType<object>(manifest)
	);
}
