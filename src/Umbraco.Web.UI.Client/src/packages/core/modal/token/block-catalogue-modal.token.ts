import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockCatalogueModalData {
	//Which unique should be sent? Document, Document Type or Data Type?
	unique: string;
	view?: 'clipboard' | 'createEmpty';
}

export interface UmbBlockCatalogueModalValue {
	content: string;
}

export const UMB_BLOCK_CATALOGUE_MODAL = new UmbModalToken<UmbBlockCatalogueModalData, UmbBlockCatalogueModalValue>(
	'Umb.Modal.BlockCatalogue',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
