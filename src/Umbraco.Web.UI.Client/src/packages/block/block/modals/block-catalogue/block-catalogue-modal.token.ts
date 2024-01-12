import { UmbBlockTypeBase } from '@umbraco-cms/backoffice/block';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockCatalogueModalData {
	blocks: Array<UmbBlockTypeBase>;
	blockGroups?: Array<{ name: string; key: string }>;
	openClipboard?: boolean;
}

export interface UmbBlockCatalogueModalValue {
	key: string;
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
