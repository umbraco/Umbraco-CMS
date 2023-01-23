import { ManifestJSType } from './load-extension.function';
import type { ManifestBase } from '@umbraco-cms/extensions-registry';

export function isManifestJSType(manifest: ManifestBase): manifest is ManifestJSType {
	return (manifest as ManifestJSType).js !== undefined;
}
