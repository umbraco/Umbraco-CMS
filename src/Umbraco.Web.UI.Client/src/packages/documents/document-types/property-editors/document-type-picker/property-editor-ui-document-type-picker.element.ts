import type { UmbInputDocumentTypeElement } from '../../components/input-document-type/input-document-type.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
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
		this.showOpenButton = config?.getValueByAlias('showOpenButton') ?? false;
	}

	@state()
	min = 0;

	@state()
	max = Infinity;

	@state()
	showOpenButton?: boolean;

	@state()
	onlyElementTypes?: boolean;

	#onChange(event: CustomEvent & { target: UmbInputDocumentTypeElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-document-type
				.min=${this.min}
				.max=${this.max}
				.value=${this.value}
				.elementTypesOnly=${this.onlyElementTypes ?? false}
				?showOpenButton=${this.showOpenButton}
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
