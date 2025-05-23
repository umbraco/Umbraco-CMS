import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type Data = object;
export type ModalValue = object;

export const EXAMPLE_ROUTED_MODAL = new UmbModalToken<Data, ModalValue>(
	'example.routed.modal', // this needs to match the alias of the modal registered in manifest.ts
	{
		modal: {
			type: 'dialog',
		},
	},
);
