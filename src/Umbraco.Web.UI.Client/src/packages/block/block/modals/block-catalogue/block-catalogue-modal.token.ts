import { UmbBlockTypeBase } from '@umbraco-cms/backoffice/block';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockCatalogueModalData {
	unique: string;
	blocks: Array<UmbBlockTypeBase>;
	blockGroups?: Array<{ name: string; key: string }>;
	view?: UmbBlockCatalogueView;
}

export type UmbBlockCatalogueView = 'clipboard' | 'createEmpty';

export interface UmbBlockCatalogueModalValue {
	content: string;
}

export const UMB_BLOCK_CATALOGUE_MODAL = new UmbModalToken<UmbBlockCatalogueModalData, UmbBlockCatalogueModalValue>(
	'Umb.Modal.BlockCatalogue',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
