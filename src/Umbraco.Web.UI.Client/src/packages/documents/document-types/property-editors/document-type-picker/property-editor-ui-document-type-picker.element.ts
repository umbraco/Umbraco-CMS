import type { UmbInputDocumentTypeElement } from '../../components/input-document-type/input-document-type.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-document-type-picker')
export class UmbPropertyEditorUIDocumentTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		this._elementTypesOnly = config.getValueByAlias('onlyPickElementTypes') ?? false;
	}

	@property({ type: Boolean, attribute: 'readonly' })
	readonly = false;

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@state()
	private _elementTypesOnly?: boolean;

	#onChange(event: CustomEvent & { target: UmbInputDocumentTypeElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-document-type
				.min=${this._min}
				.max=${this._max}
				.value=${this.value}
				.readonly=${this.readonly}
				.elementTypesOnly=${this._elementTypesOnly ?? false}
				@change=${this.#onChange}>
			</umb-input-document-type>
		`;
	}
}

export default UmbPropertyEditorUIDocumentTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-type-picker': UmbPropertyEditorUIDocumentTypePickerElement;
	}
}
