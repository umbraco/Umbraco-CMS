import { manifest as workspaceBreadcrumbKind } from './workspace-menu-breadcrumb/workspace-menu-breadcrumb.kind.js';
import { manifest as variantBreadcrumbKind } from './workspace-variant-menu-breadcrumb/workspace-variant-menu-breadcrumb.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	workspaceBreadcrumbKind,
	variantBreadcrumbKind,
];
