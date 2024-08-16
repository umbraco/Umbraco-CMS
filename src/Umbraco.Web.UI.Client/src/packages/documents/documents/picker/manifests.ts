import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.Document',
		name: 'Document Picker Search Result Item',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
];
