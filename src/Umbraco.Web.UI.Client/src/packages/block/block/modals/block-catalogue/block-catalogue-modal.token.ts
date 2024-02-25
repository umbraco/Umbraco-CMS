import type { UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel, UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';
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
