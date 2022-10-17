import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbModalService } from '../../../core/services/modal';

import './picker-layout-section.element';
import './picker-layout-user.element';

export type PickerLayout = 'section' | 'user' | 'media' | 'content';

@customElement('umb-picker')
export class UmbPickerElement extends UmbContextConsumerMixin(LitElement) {
	@property()
	public picker?: PickerLayout;

	private _modalService?: UmbModalService;

	@property({ type: Array })
	public value: Array<string> = [];

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private get sectionAlias(): string {
		switch (this.picker) {
			case 'section':
				return 'umb-picker-layout-section';
			case 'user':
				return 'umb-picker-layout-user';
			case 'media':
				return 'media';
			case 'content':
				return 'content';
			default:
				return '';
		}
	}

	private _openPicker() {
		if (!this.picker) return;

		const modalHandler = this._modalService?.open(this.sectionAlias, { type: 'sidebar', size: 'small' });
		modalHandler?.onClose().then(({ selection }: any) => {
			console.log('closed', selection);
		});
	}

	render() {
		return html`<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">
			Add
		</uui-button>`;
	}
}

export default UmbPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker': UmbPickerElement;
	}
}
