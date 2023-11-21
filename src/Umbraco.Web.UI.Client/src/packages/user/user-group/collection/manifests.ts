import {
	UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	manifests as repositoryManifests,
} from './repository/manifests.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_GROUP_COLLECTION_ALIAS = 'Umb.Collection.UserGroup';
const collection: ManifestTypes = {
	type: 'collection',
	kind: 'default',
	alias: UMB_USER_GROUP_COLLECTION_ALIAS,
	name: 'User Group Collection',
	meta: {
		repositoryAlias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [collection, ...repositoryManifests];
