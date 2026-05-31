import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbEntityBulkActionDefaultElement from '../entity-bulk-action.element.js';

export const UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.Default',
	matchKind: 'default',
	matchType: 'entityBulkAction',
	manifest: {
		type: 'entityBulkAction',
		kind: 'default',
		weight: 1000,
		element: UmbEntityBulkActionDefaultElement,
		meta: {
			icon: '',
			label: 'Default Entity Bulk Action',
		},
	},
};

export const manifest = UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST;
