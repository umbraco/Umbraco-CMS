import type { UmbInputDocumentTypeElement } from '../../components/input-document-type/input-document-type.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-document-type-picker')
export class UmbPropertyEditorUIDocumentTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (config) {
			const validationLimit = config.getValueByAlias<any>('validationLimit');
			this._limitMin = validationLimit?.min;
			this._limitMax = validationLimit?.max;

			// We have the need in Block Editors, to just pick a single ID not as an array. So for that we use the multiPicker config, which can be set to true if you wanted to be able to pick multiple.
			this._multiPicker = config.getValueByAlias('multiPicker') ?? false;
			this._onlyElementTypes = config.getValueByAlias('onlyPickElementTypes') ?? false;
		}
	}
	public get config() {
		return undefined;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;
	@state()
	private _multiPicker?: boolean;
	@state()
	private _onlyElementTypes?: boolean;

	private _onChange(event: CustomEvent) {
		const selection = (event.target as UmbInputDocumentTypeElement).selection;
		this.value = this._multiPicker ? selection.join(',') : selection[0];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	// TODO: Implement mandatory?
	render() {
		return this._multiPicker !== undefined
			? html`
					<umb-input-document-type
						@change=${this._onChange}
						.value=${this.value ?? ''}
						.min=${this._limitMin ?? 0}
						.max=${this._limitMax ?? Infinity}
						.elementTypesOnly=${this._onlyElementTypes ?? false}>
						<umb-localize key="general_add">Add</umb-localize>
					</umb-input-document-type>
				`
			: '';
	}
}

export default UmbPropertyEditorUIDocumentTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-type-picker': UmbPropertyEditorUIDocumentTypePickerElement;
	}
}
