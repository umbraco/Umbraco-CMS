import type { UmbInputMediaTypeElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-media-type-picker')
export class UmbPropertyEditorUIMediaTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	public get value(): Array<string> | string | undefined {
		return this._value;
	}
	public set value(value: Array<string> | string | undefined) {
		this._value = value;
	}
	private _value?: Array<string> | string;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (config) {
			const validationLimit = config.getValueByAlias<any>('validationLimit');
			this._limitMin = validationLimit?.min;
			this._limitMax = validationLimit?.max;

			// We have the need in Block Editors, to just pick a single ID not as an array. So for that we use the multiPicker config, which can be set to true if you wanted to be able to pick multiple.
			this._multiPicker = config.getValueByAlias('multiPicker') ?? false;
		}
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;
	@state()
	private _multiPicker?: boolean;

	private _onChange(event: CustomEvent) {
		const selectedIds = (event.target as UmbInputMediaTypeElement).selectedIds;
		this.value = this._multiPicker ? selectedIds : selectedIds[0];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
	render() {
		return this._multiPicker !== undefined
			? html`
					<umb-input-media-type
						@change=${this._onChange}
						.selectedIds=${this._multiPicker
							? (this._value as Array<string>) ?? []
							: this._value
								? [this._value as string]
								: []}
						.min=${this._limitMin ?? 0}
						.max=${this._limitMax ?? Infinity}>
						<umb-localize key="general_add">Add</umb-localize>
					</umb-input-media-type>
				`
			: '';
	}
}

export default UmbPropertyEditorUIMediaTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-type-picker': UmbPropertyEditorUIMediaTypePickerElement;
	}
}
