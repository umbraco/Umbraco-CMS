import type { UmbInputStaticFileElement } from '../../components/index.js';
import {
	type UmbPropertyEditorUiElement,
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import '../../components/input-static-file/index.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';

@customElement('umb-property-editor-ui-static-file-picker')
export class UmbPropertyEditorUIStaticFilePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#singleItemMode = false;
	// TODO: get rid of UmbServerFilePathUniqueSerializer in v.15 [NL]
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	@state()
	private _value?: string | Array<string>;

	@property({ attribute: false })
	public set value(value: string | Array<string> | undefined) {
		if (Array.isArray(value)) {
			this._value = value.map((unique) => this.#serverFilePathUniqueSerializer.toUnique(unique));
		} else if (value) {
			this._value = this.#serverFilePathUniqueSerializer.toUnique(value);
		} else {
			this._value = undefined;
		}
	}
	public get value(): string | Array<string> | undefined {
		if (Array.isArray(this._value)) {
			return this._value.map((unique) => this.#serverFilePathUniqueSerializer.toServerPath(unique) ?? '');
		} else if (this._value) {
			return this.#serverFilePathUniqueSerializer.toServerPath(this._value) ?? '';
		} else {
			return undefined;
		}
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#singleItemMode = config?.getValueByAlias<boolean>('singleItemMode') ?? false;
		const validationLimit = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min ?? 0;
		this._limitMax = this.#singleItemMode ? 1 : (validationLimit?.max ?? Infinity);
	}

	@state()
	private _limitMin: number = 0;
	@state()
	private _limitMax: number = Infinity;

	private _onChange(event: CustomEvent) {
		if (this.#singleItemMode) {
			this._value = (event.target as UmbInputStaticFileElement).selection[0];
		} else {
			this._value = (event.target as UmbInputStaticFileElement).selection;
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
