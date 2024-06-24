import type { UmbInputStaticFileElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import '../../components/input-static-file/index.js';

@customElement('umb-property-editor-ui-static-file-picker')
export class UmbPropertyEditorUIStaticFilePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string>) {
		this._value = value || [];
	}
	public get value(): Array<string> {
		return this._value;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config?.find((x) => x.alias === 'validationLimit');

		this._limitMin = (validationLimit?.value as any)?.min;
		this._limitMax = (validationLimit?.value as any)?.max;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputStaticFileElement).selection;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	// TODO: Implement mandatory?
	override render() {
		return html`
			<umb-input-static-file
				@change=${this._onChange}
				.selection=${this._value}
				.min=${this._limitMin ?? 0}
				.max=${this._limitMax ?? Infinity}></umb-input-static-file>
		`;
	}
}

export default UmbPropertyEditorUIStaticFilePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-static-file-picker': UmbPropertyEditorUIStaticFilePickerElement;
	}
}
