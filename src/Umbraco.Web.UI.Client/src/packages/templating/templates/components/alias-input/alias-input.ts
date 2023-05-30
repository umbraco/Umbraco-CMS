import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';

@customElement('umb-template-alias-input')
export class UmbTemplateAliasInputElement extends UmbLitElement {
	render() {
		return html`
        <uui-button compact @click=${this.#handleClick}>
            <uui-symbol-lock .open=${this.isOpen} ></uui-symbol-lock>
        </uui-button>
        <input .value=${this.value} ?disabled=${!this.isOpen} @input=${this.#setValue}></input>
        
        `;
	}

	@property({ type: String, attribute: 'value' })
	value = '';

	@property({ type: Boolean })
	isOpen = false;

	#setValue(event: Event) {
		event.stopPropagation();
		this.value = (event.target as HTMLInputElement).value;
	}

	#handleClick() {
		this.isOpen = !this.isOpen;
		if (!this.isOpen) {
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: inline-flex;
				align-items: center;
			}

			:host(:focus-within) {
				border-color: var(--uui-input-border-color-focus, var(--uui-color-border-emphasis, #a1a1a1));
				outline: calc(2px * var(--uui-show-focus-outline, 1)) solid var(--uui-color-focus, #3879ff);
			}

			input {
				background: transparent;
				border-color: transparent;
				font-family: inherit;
				padding: var(--uui-size-1, 3px) var(--uui-size-space-3, 9px);
				font-size: inherit;
				color: inherit;
				border-radius: 0px;
				box-sizing: border-box;
				border: none;
				background: none;
				width: 100%;
				height: 100%;
				text-align: inherit;
				outline: none;
			}

			input:disabled {
				color: #a2a1a6;
			}
		`,
	];
}

export default UmbTemplateAliasInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-alias-input': UmbTemplateAliasInputElement;
	}
}
