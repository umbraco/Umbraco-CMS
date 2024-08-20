import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';

export interface UmbBlockCatalogueModalData {
	blocks: Array<UmbBlockTypeBaseModel>;
	blockGroups?: Array<UmbBlockTypeGroup>;
	createBlockInWorkspace?: boolean;
	openClipboard?: boolean;
	blockOriginData: UmbBlockWorkspaceData['originData'];
}

export type UmbBlockCatalogueModalValue =
	| {
			create?: {
				contentElementTypeKey: string;
			};
	  }
	| undefined;

export const UMB_BLOCK_CATALOGUE_MODAL = new UmbModalToken<UmbBlockCatalogueModalData, UmbBlockCatalogueModalValue>(
	'Umb.Modal.BlockCatalogue',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
