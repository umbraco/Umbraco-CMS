import { html, render } from 'lit';

//TODO consider splitting this into two separate handlers
export default class UmbModalHandler {
	private _closeResolver: any;
	private _closePromise: any;

	public element?: any;
	public key: string;
	public modal: any;

	constructor(elementName: string, modalElementName: string, modalOptions?: any) {
		this.key = Date.now().toString(); //TODO better key
		this._createLayoutElement(elementName, modalElementName, modalOptions);
		this._closePromise = new Promise((resolve) => {
			this._closeResolver = resolve;
		});
	}

	private _createLayoutElement(elementName: string, modalElementName: string, modalOptions?: any) {
		this.modal = document.createElement(modalElementName);
		this.modal.addEventListener('close-end', () => {
			this._closeResolver();
		});

		if (modalOptions) {
			// Apply modal options as attributes on the modal
			Object.keys(modalOptions).forEach((option) => {
				this.modal.setAttribute(option, modalOptions[option]);
			});
		}

		this.element = document.createElement(elementName);
		this.modal.appendChild(this.element);
		this.element.modalHandler = this;
	}

	public close(...args: any) {
		this._closeResolver(...args);
		this.modal.close();
	}

	public get onClose(): Promise<any> {
		return this._closePromise;
	}
}
