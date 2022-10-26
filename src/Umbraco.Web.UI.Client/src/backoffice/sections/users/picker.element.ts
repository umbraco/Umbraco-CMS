import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbModalOptions, UmbModalService } from '../../../core/services/modal';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

//TODO: These should probably be imported dynamically.
import './picker-layout-section.element';
import './picker-layout-user-group.element';
import './picker-layout-user.element';

export interface UmbPickerData {
	multiple: boolean;
	selection: Array<string>;
}

@customElement('umb-picker')
export class UmbPickerElement extends UmbContextConsumerMixin(LitElement) {
	@property({ type: Array })
	public value: Array<string> = [];

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
				selection: [...this.value],
			},
		});
		modalHandler?.onClose().then((data: UmbPickerData) => {
			if (data) {
				this.value = data.selection;
				this.selectionUpdated();
			}
		});
	}

	protected removeFromSelection(key: string) {
		this.value = this.value.filter((k) => k !== key);
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
