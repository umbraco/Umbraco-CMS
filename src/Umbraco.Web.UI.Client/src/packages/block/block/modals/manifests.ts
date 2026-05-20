export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.BlockCatalogue',
		name: 'Block Catalogue Modal',
		element: () => import('./block-catalogue/block-catalogue-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.BlockTransferToElementLibrary',
		name: 'Transfer Block To Element Library Modal',
		element: () => import('./transfer-to-element-library/transfer-to-element-library-modal.element.js'),
	},
];
