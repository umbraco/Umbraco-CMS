import type { UUIDialogElement } from "@umbraco-cms/backoffice/external/uui";
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
		elementFactory : ()=> {
			// returning the custom modal element 
			const modalDialogElement = document.createElement('example-modal-element');
			const dialogElement: UUIDialogElement = document.createElement('uui-dialog');
        	modalDialogElement.appendChild(dialogElement);
        	return modalDialogElement;
		}
	}
	
});
