import { manifests as dataTypeTreeManifests } from './data-types/manifests';
import { manifests as dictionaryTreeManifests } from './dictionary/manifests';
import { manifests as documentBlueprintTreeManifests } from './document-blueprints/manifests';
import { manifests as documentTypeTreeManifests } from './document-types/manifests';
import { manifests as documentTreeManifests } from './documents/manifests';
import { manifests as extensionTreeManifests } from './extensions/manifests';
import { manifests as languageTreeManifests } from './languages/manifests';
import { manifests as mediaTreeManifests } from './media/manifests';
import { manifests as mediaTypeTreeManifests } from './media-types/manifests';
import { manifests as memberGroupTreeManifests } from './member-groups/manifests';
import { manifests as memberTypesTreeManifests } from './member-types/manifests';

import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

export const manifests: Array<ManifestTree | ManifestTreeItemAction> = [
	...dataTypeTreeManifests,
	...dictionaryTreeManifests,
	...documentBlueprintTreeManifests,
	...documentTypeTreeManifests,
	...documentTreeManifests,
	...extensionTreeManifests,
	...languageTreeManifests,
	...mediaTreeManifests,
	...mediaTypeTreeManifests,
	...memberGroupTreeManifests,
	...memberTypesTreeManifests,
];
