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
		this.min = minMax?.min ?? 0;
		this.max = minMax?.max ?? Infinity;

		this.onlyElementTypes = config.getValueByAlias('onlyPickElementTypes') ?? false;
	}

	@property({ type: Boolean, attribute: 'readonly' })
	readonly = false;

	@state()
	min = 0;

	@state()
	max = Infinity;

	@state()
	onlyElementTypes?: boolean;

	#onChange(event: CustomEvent & { target: UmbInputDocumentTypeElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-document-type
				.min=${this.min}
				.max=${this.max}
				.value=${this.value}
				.readonly=${this.readonly}
				.elementTypesOnly=${this.onlyElementTypes ?? false}
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
