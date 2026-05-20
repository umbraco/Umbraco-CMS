import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

// This module is a virtual Vite module at build time (see devops/build/vite-plugin-unified-manifests.ts).
// This source file exists only to satisfy TypeScript path resolution during `npm run compile`.
// The actual runtime module is assembled by the Vite plugin from all packages' manifests.ts files.

export declare const allManifests: Array<UmbExtensionManifest | UmbExtensionManifestKind>;
