// TODO: temp file until we have a way to register from each extension
import { manifests as extensionTreeManifests } from './core/extensions/tree/manifests';
import { manifests as memberTypesTreeManifests } from './members/member-types/tree/manifests';
import { manifests as mediaTypeTreeManifests } from './media/media-types/tree/manifests';

import type { ManifestTree, ManifestTreeItemAction, ManifestWorkspace } from '@umbraco-cms/models';

export const manifests: Array<ManifestTree | ManifestTreeItemAction | ManifestWorkspace> = [
	...extensionTreeManifests,
	...mediaTypeTreeManifests,
	...memberTypesTreeManifests,
];
