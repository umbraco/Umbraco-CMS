import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config?: UmbDataTypePropertyCollection;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _openModal() {
		this._modalContext?.open(UMB_ICON_PICKER_MODAL);
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

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIIconPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-icon-picker': UmbPropertyEditorUIIconPickerElement;
	}
}
