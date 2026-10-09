import { CSSResultGroup, css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-back-to-login-button')
export default class UmbBackToLoginButtonElement extends UmbLitElement {
	render() {
		return html`
			<uui-button type="button" @click=${this.#handleClick} compact>
				<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
					<path
						fill="currentColor"
						d="M7.82843 10.9999H20V12.9999H7.82843L13.1924 18.3638L11.7782 19.778L4 11.9999L11.7782 4.22168L13.1924 5.63589L7.82843 10.9999Z"></path>
				</svg>
				<span><umb-localize key="auth_returnToLogin">Return to login form</umb-localize></span>
			</uui-button>
		`;
	}

	#handleClick() {
		this.dispatchEvent(new CustomEvent('umb-login-flow', { composed: true, detail: { flow: 'login', status: null } }));
	}

	static styles: CSSResultGroup = [
		css`
			:host {
				display: flex;
				align-items: center;
				justify-content: center;
			}
			uui-button {
				--uui-button-height: auto;
				--uui-button-background-color-hover: transparent;
			}
			uui-button svg {
				width: 1rem;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-back-to-login-button': UmbBackToLoginButtonElement;
	}
}
