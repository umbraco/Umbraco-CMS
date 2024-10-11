import { manifests as entrypoints } from './entrypoints/manifest';
import { manifests as dashboards } from './dashboards/manifest';

// Job of the bundle is to collate all the manifests from different parts of the extension and load other manifests
// We load this bundle from umbraco-package.json
export const manifests: Array<UmbExtensionManifest> = [
	...entrypoints,
	...dashboards,
];
