import type { ManifestTypes } from './models/index.js';
import type { ManifestKind} from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

export type UmbBackofficeManifestKind = ManifestKind<ManifestTypes>;
export type UmbBackofficeExtensionRegistry = UmbExtensionRegistry<ManifestTypes>;

export const umbExtensionsRegistry = new UmbExtensionRegistry<ManifestTypes>() as UmbBackofficeExtensionRegistry;
