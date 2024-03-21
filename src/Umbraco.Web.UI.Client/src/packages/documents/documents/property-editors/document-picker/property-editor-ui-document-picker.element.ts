import type { UmbInputDocumentElement } from '../../components/input-document/input-document.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIDocumentPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config?.find((x) => x.alias === 'validationLimit');

		this._limitMin = (validationLimit?.value as any)?.min;
		this._limitMax = (validationLimit?.value as any)?.max;
	}
	public get config() {
		return undefined;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputDocumentElement).selection.join(',');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	// TODO: Implement mandatory?
	render() {
		return html`
			<umb-input-document
				@change=${this._onChange}
				.value=${this.value ?? ''}
				.min=${this._limitMin ?? 0}
				.max=${this._limitMax ?? Infinity}>
				<umb-localize key="general_add">Add</umb-localize>
			</umb-input-document>
		`;
	}
}

export default UmbPropertyEditorUIDocumentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-picker': UmbPropertyEditorUIDocumentPickerElement;
	}
}
