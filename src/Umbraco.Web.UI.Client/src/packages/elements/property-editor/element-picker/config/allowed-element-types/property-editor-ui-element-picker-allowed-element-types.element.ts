import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';

import '@umbraco-cms/backoffice/document-type';

@customElement('umb-property-editor-ui-element-picker-allowed-element-types')
export class UmbPropertyEditorUIElementPickerAllowedElementTypesElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property()
	public set value(value: string) {
		this.#selection = value ? value.split(',') : [];
	}
	public get value(): string {
		return this.#selection.join(',');
	}

	#selection: Array<string> = [];

	#onChange(event: CustomEvent & { target: { selection: string[] } }) {
		this.value = event.target.selection.join(',');
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-document-type
			.elementTypesOnly=${true}
			.selection=${this.#selection}
			@change=${this.#onChange}></umb-input-document-type>`;
	}
}

export default UmbPropertyEditorUIElementPickerAllowedElementTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-element-picker-allowed-element-types': UmbPropertyEditorUIElementPickerAllowedElementTypesElement;
	}
}
