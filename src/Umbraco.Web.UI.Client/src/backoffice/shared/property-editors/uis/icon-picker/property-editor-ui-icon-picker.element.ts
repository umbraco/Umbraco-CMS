import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../core/modal';
import { UmbLitElement } from '@umbraco-cms/context-api';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
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
