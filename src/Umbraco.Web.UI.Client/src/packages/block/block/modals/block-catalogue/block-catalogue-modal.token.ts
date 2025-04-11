import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbBlockTypeGroup, UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { UmbClipboardEntryDetailModel } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockWorkspaceData } from '../../workspace/index.js';

export interface UmbBlockCatalogueModalData {
	blocks: Array<UmbBlockTypeBaseModel>;
	blockGroups?: Array<UmbBlockTypeGroup>;
	createBlockInWorkspace?: boolean;
	openClipboard?: boolean;
	clipboardFilter?: (clipboardDetailEntryModel: UmbClipboardEntryDetailModel) => Promise<boolean>;
	originData: UmbBlockWorkspaceData['originData'];
}

export type UmbBlockCatalogueModalValue =
	| {
			create?: {
				contentElementTypeKey: string;
			};
			clipboard?: {
				selection: Array<string>;
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
