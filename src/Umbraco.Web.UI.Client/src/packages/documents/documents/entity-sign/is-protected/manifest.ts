import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: UmbExtensionManifest = {
	type: 'entitySign',
	kind: 'icon',
	alias: 'Umb.EntitySign.Document.IsProtected',
	name: 'Is Protected Document Entity Sign',
	forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	forEntityFlags: ['Umb.IsProtected'],
	meta: {
		iconName: 'icon-lock',
		label: 'Protected',
	},
};
