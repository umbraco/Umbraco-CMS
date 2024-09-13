import type { ManifestKind } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

export type UmbBackofficeManifestKind = ManifestKind<UmbExtensionManifest>;
export type UmbBackofficeExtensionRegistry = UmbExtensionRegistry<UmbExtensionManifest>;

export const umbExtensionsRegistry = new UmbExtensionRegistry<UmbExtensionManifest>() as UmbBackofficeExtensionRegistry;
