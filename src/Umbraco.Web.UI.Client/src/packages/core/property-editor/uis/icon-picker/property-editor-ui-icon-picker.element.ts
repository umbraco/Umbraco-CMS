import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_ICON_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _openModal() {
		// TODO: Modal should know the current selected one, and bring back the newly selected one.
		this._modalContext?.open(UMB_ICON_PICKER_MODAL);
	}

	// TODO: We should show the current picked icon.
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

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIIconPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-icon-picker': UmbPropertyEditorUIIconPickerElement;
	}
}
