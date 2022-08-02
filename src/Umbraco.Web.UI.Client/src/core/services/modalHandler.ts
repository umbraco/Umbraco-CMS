export default class UmbModalHandler {
	private _elementName;
	private _closeResolver: any;
	private _closePromise: any;

	public element?: any;
	public key: string;
	public modal: any;

	constructor(elementName: string, modalElementName: string) {
		this.key = Date.now().toString(); //TODO better key
		this._elementName = elementName;
		this._createLayoutElement(modalElementName);
		this._closePromise = new Promise((resolve) => {
			this._closeResolver = resolve;
		});
	}

	private _createLayoutElement(modalElementName: string) {
		this.modal = document.createElement(modalElementName);
		this.modal.addEventListener('close-end', () => {
			this._closeResolver();
		});
		this.modal.size = 'small'; //TODO make meta object for settings
		this.element = document.createElement(this._elementName);
		this.modal.appendChild(this.element);
		this.element.modalHandler = this;
	}

	public close() {
		this.modal.close();
	}

	public get onClose(): Promise<any> {
		return this._closePromise;
	}
}
