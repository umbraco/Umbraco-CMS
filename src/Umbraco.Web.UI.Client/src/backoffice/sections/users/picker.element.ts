import { html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbModalOptions, UmbModalService } from '../../../core/services/modal';

import './picker-layout-section.element';
import './picker-layout-user.element';

export interface UmbPickerData {
	multiple: boolean;
	selection: Array<string>;
}

@customElement('umb-picker')
export class UmbPickerElement extends UmbContextConsumerMixin(LitElement) {
	@property({ type: Array })
	public selection: Array<string> = [];

	protected pickerLayout?: string;
	protected pickerOptions: UmbModalOptions<UmbPickerData> = {
		type: 'sidebar',
		size: 'small',
		data: {
			multiple: true,
			selection: [],
		},
	};
	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _openPicker() {
		if (!this.pickerLayout) return;

		const modalHandler = this._modalService?.open(this.pickerLayout, {
			...this.pickerOptions,
			data: {
				...this.pickerOptions.data,
				selection: [...this.selection],
			},
		});
		modalHandler?.onClose().then((data: UmbPickerData) => {
			if (data) {
				this.selection = data.selection;
				this.selectionUpdated();
			}
			console.log('modal closed', data);
		});
	}

	protected removeFromSelection(key: string) {
		this.selection = this.selection.filter((k) => k !== key);
		this.selectionUpdated();
	}

	protected selectionUpdated() {
		// override this method to react to selection changes
	}

	protected renderButton() {
		return html`<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">
			Add
		</uui-button>`;
	}
	protected renderContent() {
		return html``;
	}

	render() {
		return html`${this.renderButton()} ${this.renderContent()}`;
	}
}

export default UmbPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker': UmbPickerElement;
	}
}
