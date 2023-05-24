import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

@customElement('umb-property-editor-ui-text-box')
export class UmbPropertyEditorUITextBoxElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	#defaultType = 'text';

	@property()
	value = '';

	@state()
	private _type?: string;

	@state()
	private _maxChars?: number;

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		this._type = config.getValueByAlias('inputType') ?? this.#defaultType;
		this._maxChars = config.getValueByAlias('maxChars');
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-input
			.value=${this.value}
			type="${this._type}"
			maxlength="${ifDefined(this._maxChars)}"
			@input=${this.onInput}></uui-input>`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUITextBoxElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-text-box': UmbPropertyEditorUITextBoxElement;
	}
}
