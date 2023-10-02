import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

type UuiInputTypeType = typeof UUIInputElement.prototype.type;

@customElement('umb-property-editor-ui-text-box')
export class UmbPropertyEditorUITextBoxElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	#defaultType: UuiInputTypeType = 'text';

	@property()
	value = '';

	@state()
	private _type: UuiInputTypeType = this.#defaultType;

	@state()
	private _maxChars?: number;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._type = config?.getValueByAlias<UuiInputTypeType>('inputType') ?? this.#defaultType;
		this._maxChars = config?.getValueByAlias('maxChars');
	}

	private onChange(e: Event) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-input
			.value=${this.value ?? ''}
			.type=${this._type}
			maxlength=${ifDefined(this._maxChars)}
			@change=${this.onChange}></uui-input>`;
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
