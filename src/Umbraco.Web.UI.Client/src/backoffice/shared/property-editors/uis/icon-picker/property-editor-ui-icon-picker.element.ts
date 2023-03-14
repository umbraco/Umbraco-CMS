import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UMB_ICON_PICKER_MODAL_TOKEN } from '../../../modals/icon-picker';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _openModal() {
		this._modalContext?.open(UMB_ICON_PICKER_MODAL_TOKEN);
	}

	render() {
		return html`
			<uui-button
				label="open-icon-picker"
				look="secondary"
				@click=${this._openModal}
				style="margin-right: var(--uui-size-space-3)">
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
