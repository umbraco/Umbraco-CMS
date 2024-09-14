export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.BlockCatalogue',
		name: 'Block Catalogue Modal',
		element: () => import('./block-catalogue/block-catalogue-modal.element.js'),
	},
];
