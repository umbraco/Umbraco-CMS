import { UmbCreateUserCollectionAction } from './create-user.collection-action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	alias: 'Umb.CollectionAction.User.Create',
	api: UmbCreateUserCollectionAction,
	meta: {
		label: 'Create User',
	},
};

export const manifests = [createManifest];
