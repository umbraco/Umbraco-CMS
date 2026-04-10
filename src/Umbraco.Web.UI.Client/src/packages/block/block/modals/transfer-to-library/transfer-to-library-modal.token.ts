import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockTransferToLibraryModalData {}

export interface UmbBlockTransferToLibraryModalValue {
	name: string;
	parentUnique: string | null;
}

export const UMB_BLOCK_TRANSFER_TO_LIBRARY_MODAL = new UmbModalToken<
	UmbBlockTransferToLibraryModalData,
	UmbBlockTransferToLibraryModalValue
>('Umb.Modal.BlockTransferToLibrary', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
