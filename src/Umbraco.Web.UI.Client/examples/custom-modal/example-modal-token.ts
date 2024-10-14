import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

export interface ExampleModalData {
	unique: string | null;
}

export interface ExampleModalResult {
	text : string;
}

export const EXAMPLE_MODAL_TOKEN = new UmbModalToken<
ExampleModalData,
ExampleModalResult
>('example.modal.custom.element', {
	modal : {
		type : 'custom',
		element: () => import('./example-custom-modal-element.element.js'),
	}
});
