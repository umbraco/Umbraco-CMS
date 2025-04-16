import type { ManifestKind } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

export type UmbExtensionManifestKind = ManifestKind<UmbExtensionManifest>;
export type UmbBackofficeExtensionRegistry = UmbExtensionRegistry<UmbExtensionManifest, UmbExtensionConditionConfig>;

export const umbExtensionsRegistry = new UmbExtensionRegistry<
	UmbExtensionManifest,
	UmbExtensionConditionConfig
>() as UmbBackofficeExtensionRegistry;

/**
 * @deprecated Use `UmbExtensionManifestKind` instead.
 */
export type UmbBackofficeManifestKind = ManifestKind<UmbExtensionManifest>;
