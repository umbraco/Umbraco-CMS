import type { UmbBlockTypeBaseModel, UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockCatalogueModalData {
	blocks: Array<UmbBlockTypeBaseModel>;
	blockGroups?: Array<{ name: string; key: string }>;
	openClipboard?: boolean;
	blockOriginData: UmbBlockWorkspaceData;
}

export type UmbBlockCatalogueModalValue = never;

export const UMB_BLOCK_CATALOGUE_MODAL = new UmbModalToken<UmbBlockCatalogueModalData, undefined>(
	'Umb.Modal.BlockCatalogue',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
