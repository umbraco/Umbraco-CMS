import type { ManifestKind } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

export type UmbBackofficeManifestKind = ManifestKind<UmbManifestTypes>;
export type UmbBackofficeExtensionRegistry = UmbExtensionRegistry<UmbManifestTypes>;

export const umbExtensionsRegistry = new UmbExtensionRegistry<UmbManifestTypes>() as UmbBackofficeExtensionRegistry;
