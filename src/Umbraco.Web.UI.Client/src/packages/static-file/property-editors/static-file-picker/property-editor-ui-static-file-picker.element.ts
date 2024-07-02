import type { UmbInputStaticFileElement } from '../../components/index.js';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import '../../components/input-static-file/index.js';

@customElement('umb-property-editor-ui-static-file-picker')
export class UmbPropertyEditorUIStaticFilePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _singleItemMode = false;

	@state()
	private _value?: string | Array<string>;

	@property({ attribute: false })
	public set value(value: string | Array<string> | undefined) {
		this._value = value;
	}
	public get value() {
		return this._value;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._singleItemMode = config?.getValueByAlias<boolean>('singleItemMode') ?? false;
		const validationLimit = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	private _onChange(event: CustomEvent) {
		if (this._singleItemMode) {
			this.value = (event.target as UmbInputStaticFileElement).selection[0];
		} else {
			this.value = (event.target as UmbInputStaticFileElement).selection;
		}
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	// TODO: Implement mandatory?
	override render() {
		return html`
			<umb-input-static-file
				.selection=${this._value ? (Array.isArray(this._value) ? this._value : [this._value]) : []}
				.min=${this._limitMin ?? 0}
				.max=${this._limitMax ?? Infinity}
				@change=${this._onChange}></umb-input-static-file>
		`;
	}
}

export default UmbPropertyEditorUIStaticFilePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-static-file-picker': UmbPropertyEditorUIStaticFilePickerElement;
	}
}
