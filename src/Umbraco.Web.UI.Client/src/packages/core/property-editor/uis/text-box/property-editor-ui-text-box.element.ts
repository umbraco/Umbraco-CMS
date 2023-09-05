import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

@customElement('umb-property-editor-ui-text-box')
export class UmbPropertyEditorUITextBoxElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	#defaultType = 'text';

	@property()
	value = '';

	@state()
	private _type?: string;

	@state()
	private _maxChars?: number;

	@property({ attribute: false })
	public set config(config: UmbDataTypeConfigCollection | undefined) {
		this._type = config?.getValueByAlias('inputType') ?? this.#defaultType;
		this._maxChars = config?.getValueByAlias('maxChars');
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-input
			.value=${this.value ?? ''}
			type="${this._type}"
			maxlength="${ifDefined(this._maxChars)}"
			@input=${this.onInput}></uui-input>`;
	}

	static styles = [
		UmbTextStyles,
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
