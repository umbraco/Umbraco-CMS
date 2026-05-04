export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.BlockCatalogue',
		name: 'Block Catalogue Modal',
		element: () => import('./block-catalogue/block-catalogue-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.BlockTransferToLibrary',
		name: 'Block Transfer To Library Modal',
		element: () => import('./transfer-to-library/transfer-to-library-modal.element.js'),
	},
];
