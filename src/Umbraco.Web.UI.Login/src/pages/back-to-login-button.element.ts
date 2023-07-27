import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-back-to-login-button')
export default class UmbBackToLoginButtonElement extends LitElement {
	render() {
		return html`
			<a href="login">
				<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
					<path
						fill="currentColor"
						d="M7.82843 10.9999H20V12.9999H7.82843L13.1924 18.3638L11.7782 19.778L4 11.9999L11.7782 4.22168L13.1924 5.63589L7.82843 10.9999Z"></path>
				</svg>
				<span>Back to log in</span>
			</a>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				align-items: center;
				justify-content: center;
			}
			a {
				height: 1rem;
				color: #868686; /* TODO Change to uui color when uui gets a muted text variable */
				font-size: 14px;
				gap: var(--uui-size-space-1);
				align-self: center;
				text-decoration: none;
				display: inline-flex;
				line-height: 1;
				font-weight: 600;
			}
			a svg {
				width: 1rem;
			}
			a:hover {
				color: var(--uui-color-interactive-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-back-to-login-button': UmbBackToLoginButtonElement;
	}
}
