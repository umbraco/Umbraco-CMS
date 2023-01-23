import type { ManifestBase } from '@umbraco-cms/extensions-registry';
import { ManifestJSType } from './load-extension.function';

export function isManifestJSType(manifest: ManifestBase): manifest is ManifestJSType {
	return (manifest as ManifestJSType).js !== undefined;
}
