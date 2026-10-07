import type { UmbMockSetManifest } from '../mock-data-set.types.js';
import { manifest as defaultSet } from './default/manifest.js';
import { manifest as kitchenSink } from './kitchen-sink/manifest.js';
import { manifest as userPermissions } from './user-permissions/manifest.js';
import { manifest as documents } from './documents/manifest.js';
import { manifest as blocks } from './blocks/manifest.js';

export const manifests: Array<UmbMockSetManifest> = [defaultSet, kitchenSink, userPermissions, documents, blocks];
