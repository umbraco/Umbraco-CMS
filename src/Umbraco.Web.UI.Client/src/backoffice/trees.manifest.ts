// TODO: temp file until we have a way to register from each extension

import { manifests as dataTypeTreeManifests } from './core/data-types/manifests';
import { manifests as extensionTreeManifests } from './core/extensions/tree/manifests';
import { manifests as languageTreeManifests } from './core/languages/tree/manifests';
import { manifests as dictionaryTreeManifests } from './translation/dictionary/tree/dictionary-tree.manifest';
import { manifests as documentTreeManifests } from './documents/documents/tree/manifests';
import { manifests as memberTypesTreeManifests } from './members/member-types/tree/manifests';
import { manifests as memberGroupTreeManifests } from './members/member-groups/tree/manifests';
import { manifests as mediaTreeManifests } from './media/media/tree/manifests';
import { manifests as mediaTypeTreeManifests } from './media/media-types/tree/manifests';

import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

export const manifests: Array<ManifestTree | ManifestTreeItemAction> = [
	...dataTypeTreeManifests,
	...dictionaryTreeManifests,
	...documentTreeManifests,
	...extensionTreeManifests,
	...languageTreeManifests,
	...mediaTreeManifests,
	...mediaTypeTreeManifests,
	...memberGroupTreeManifests,
	...memberTypesTreeManifests,
];
