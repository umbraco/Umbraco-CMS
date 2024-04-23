import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbInputMemberElement } from '@umbraco-cms/backoffice/member';

/**
 * @element umb-property-editor-ui-member-picker
 */
@customElement('umb-property-editor-ui-member-picker')
export class UmbPropertyEditorUIMemberPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<NumberRangeValueType>('validationLimit');
		this.min = minMax?.min ?? 0;
		this.max = minMax?.max ?? Infinity;
	}

	@state()
	min = 0;

	@state()
	max = Infinity;

	#onChange(event: CustomEvent & { target: UmbInputMemberElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-member
				.min=${this.min}
				.max=${this.max}
				.value=${this.value ?? ''}
				@change=${this.#onChange}></umb-input-member>
		`;
	}
}

export default UmbPropertyEditorUIMemberPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-member-picker': UmbPropertyEditorUIMemberPickerElement;
	}
}
