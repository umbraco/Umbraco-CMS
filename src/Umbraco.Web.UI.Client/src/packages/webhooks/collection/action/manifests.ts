import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestCollectionAction = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Webhook Collection Action',
	alias: 'Umb.CollectionAction.Webhook.Create',
	weight: 200,
	meta: {
		label: 'Create',
		href: 'section/settings/workspace/webhook/create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Webhook',
		},
	],
};

export const manifests = [createManifest];
