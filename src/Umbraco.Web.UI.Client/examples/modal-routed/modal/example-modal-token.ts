import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

export type Data = {}
export type RetData = {}

export const EXAMPLE_ROUTED_MODAL = new UmbModalToken<
Data,
RetData
>('example.routed.modal', // this needs to match the alias of the modal registered in manifest.ts
{
	modal : {
		type : 'dialog',
    size : 'full'
	}
});
