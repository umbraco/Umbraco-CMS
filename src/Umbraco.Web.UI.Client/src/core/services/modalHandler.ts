export default class UmbModalHandler {
	private _elementName;
	private _closeResolver: any;
	private _closePromise: any;

	public element?: any;
	public key: string;
	public modal: any;

	constructor(elementName: string) {
		this.key = Date.now().toString(); //TODO better key
		this._elementName = elementName;
		this._createLayoutElement();
		this._closePromise = new Promise((res) => {
			this._closeResolver = res;
		});
	}

	private _createLayoutElement() {
		this.modal = document.createElement('uui-modal-sidebar');
		this.modal.size = 'small'; //TODO make meta object for settings
		this.element = document.createElement(this._elementName);
		this.modal.appendChild(this.element);
		this.element.modalHandler = this;
	}

	public close(...args: any) {
		this._closeResolver(...args);
	}

	public onClose(): Promise<any> {
		return this._closePromise;
	}
}
