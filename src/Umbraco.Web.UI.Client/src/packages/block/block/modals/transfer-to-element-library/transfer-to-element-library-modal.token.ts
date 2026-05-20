import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockTransferToElementLibraryModalData {}

export interface UmbBlockTransferToElementLibraryModalValue {
	name: string;
	parentUnique: string | null;
}

export const UMB_BLOCK_TRANSFER_TO_ELEMENT_LIBRARY_MODAL = new UmbModalToken<
	UmbBlockTransferToElementLibraryModalData,
	UmbBlockTransferToElementLibraryModalValue
>('Umb.Modal.BlockTransferToElementLibrary', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
