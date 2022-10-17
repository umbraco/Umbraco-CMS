import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { UmbModalService } from '../../../core/services/modal';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _openModal() {
		this._modalService?.iconPicker();
	}

	render() {
		return html`
			<uui-button label="open-icon-picker" look="secondary" @click=${this._openModal} style="margin-right: 9px;">
				Pick an icon
			</uui-button>
		`;
	}
}

export default UmbPropertyEditorUIIconPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-icon-picker': UmbPropertyEditorUIIconPickerElement;
	}
}
