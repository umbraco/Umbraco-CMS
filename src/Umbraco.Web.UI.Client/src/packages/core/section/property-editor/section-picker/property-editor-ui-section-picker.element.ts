import type { UmbInputSectionElement } from '../../components/input-section/input-section.element.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-section-picker')
export class UmbSectionPickerPropertyEditorUIElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ attribute: false })
	public value: Array<string> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;
	}

	@state() private _min = 0;
	@state() private _max = Infinity;

	#onChange(event: CustomEvent & { target: UmbInputSectionElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-section .selection=${this.value} .min=${this._min} .max=${this._max} @change=${this.#onChange}>
			</umb-input-section>
		`;
	}
}

export { UmbSectionPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-section-picker': UmbSectionPickerPropertyEditorUIElement;
	}
}
