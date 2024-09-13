import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';

export const createManifest: ManifestCollectionAction = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Webhook Collection Action',
	alias: 'Umb.CollectionAction.Webhook.Create',
	weight: 200,
	meta: {
		label: '#general_create',
		href: 'section/settings/workspace/webhook/create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Webhook',
		},
	],
};

export const manifests: Array<ManifestTypes> = [createManifest];
