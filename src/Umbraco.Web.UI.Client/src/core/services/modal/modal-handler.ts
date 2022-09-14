import { UUIDialogElement } from '@umbraco-ui/uui';
import { UUIModalDialogElement } from '@umbraco-ui/uui-modal-dialog';
import { UUIModalSidebarElement, UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { v4 as uuidv4 } from 'uuid';

import { UmbModalOptions } from './modal.service';

//TODO consider splitting this into two separate handlers
export class UmbModalHandler {
	private _closeResolver: any;
	private _closePromise: any;

	public element: UUIModalDialogElement | UUIModalSidebarElement;
	public key: string;
	public type: string;
	public size: UUIModalSidebarSize;

	constructor(element: string | HTMLElement, options?: UmbModalOptions<unknown>) {
		this.key = uuidv4();

		this.type = options?.type || 'dialog';
		this.size = options?.size || 'small';
		this.element = this._createElement(element, options);

		this._closePromise = new Promise((resolve) => {
			this._closeResolver = resolve;
		});
	}

	private _createElement(element: string | HTMLElement, options?: UmbModalOptions<unknown>) {
		const layoutElement = this._createLayoutElement(element, options?.data);
		return this.type === 'sidebar'
			? this._createSidebarElement(layoutElement)
			: this._createDialogElement(layoutElement);
	}

	private _createSidebarElement(layoutElement: HTMLElement) {
		const sidebarElement = document.createElement('uui-modal-sidebar');
		sidebarElement.appendChild(layoutElement);
		sidebarElement.size = this.size;
		return sidebarElement;
	}

	private _createDialogElement(layoutElement: HTMLElement) {
		const modalDialogElement = document.createElement('uui-modal-dialog');
		const dialogElement: UUIDialogElement = document.createElement('uui-dialog');
		modalDialogElement.appendChild(dialogElement);
		dialogElement.appendChild(layoutElement);
		return modalDialogElement;
	}

	private _createLayoutElement(element: string | HTMLElement, data: unknown) {
		const layoutElement: any = element instanceof HTMLElement ? element : document.createElement(element);
		layoutElement.data = data;
		layoutElement.modalHandler = this;
		return layoutElement;
	}

	public close(...args: any) {
		this._closeResolver(...args);
		this.element.close();
	}

	public get onClose(): Promise<any> {
		return this._closePromise;
	}
}
