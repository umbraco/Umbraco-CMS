import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Document.IsDraft',
		name: 'Draft Document Entity Sign',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: { iconName: 'icon-edit', label: 'Draft' },
	},
];
