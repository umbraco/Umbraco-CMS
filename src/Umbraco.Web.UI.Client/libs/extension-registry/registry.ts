import type { ManifestTypes } from './models';
import { ManifestKind, UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

export type UmbBackofficeManifestKind = ManifestKind<ManifestTypes>;
export type UmbBackofficeExtensionRegistry = UmbExtensionRegistry<ManifestTypes>;

export const umbExtensionsRegistry = new UmbExtensionRegistry<ManifestTypes>() as UmbBackofficeExtensionRegistry;
