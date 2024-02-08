import type { UmbBlockTypeBaseModel, UmbBlockTypeGroup, UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockCatalogueModalData {
	blocks: Array<UmbBlockTypeBaseModel>;
	blockGroups?: Array<UmbBlockTypeGroup>;
	openClipboard?: boolean;
	blockOriginData: UmbBlockWorkspaceData['originData'];
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
