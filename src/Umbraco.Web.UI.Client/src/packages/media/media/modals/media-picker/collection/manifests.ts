import { UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS } from '../../../collection/repository/index.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { UmbMediaPickerCollectionContext } from './media-picker-collection.context.js';
import { UMB_MEDIA_PICKER_COLLECTION_ALIAS } from './index.js';
import type { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	alias: UMB_MEDIA_PICKER_COLLECTION_ALIAS,
	name: 'Media Picker Collection',
	api: UmbMediaPickerCollectionContext,
	element: () => import('./media-picker-collection.element.js'),
	meta: {
		repositoryAlias: UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [collectionManifest, ...collectionViewManifests];
