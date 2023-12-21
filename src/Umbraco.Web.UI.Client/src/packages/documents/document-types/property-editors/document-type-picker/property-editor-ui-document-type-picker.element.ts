import type { UmbInputDocumentTypeElement } from '../../components/input-document-type/input-document-type.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-document-type-picker')
export class UmbPropertyEditorUIDocumentTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (config) {
			const validationLimit = config.getValueByAlias<any>('validationLimit');
			this._limitMin = validationLimit?.min;
			this._limitMax = validationLimit?.max;

			this._onlyElementTypes = config.getValueByAlias('onlyPickElementTypes');
		}
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;
	@state()
	private _onlyElementTypes?: boolean;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputDocumentTypeElement).selectedIds;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	// TODO: Implement mandatory?
	render() {
		return html`
			<umb-input-document-type
				@change=${this._onChange}
				.selectedIds=${this._value}
				.min=${this._limitMin ?? 0}
				.max=${this._limitMax ?? Infinity}
				.element-types-only=${this._onlyElementTypes}
				>Add</umb-input-document-type
			>
		`;
	}
}

export default UmbPropertyEditorUIDocumentTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-type-picker': UmbPropertyEditorUIDocumentTypePickerElement;
	}
}
