import { css, CSSResultGroup, html, LitElement, unsafeCSS } from 'lit';
import { customElement } from 'lit/decorators.js';
import logoImg from '/umbraco_logomark_white.svg';
import loginImg from '/login.jpeg';

@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
	static styles: CSSResultGroup = [
		css`
			#background {
				position: fixed;
				overflow: hidden;
				background-position: 50%;
				background-repeat: no-repeat;
				background-size: cover;
				background-image: url('${unsafeCSS(loginImg)}');
				width: 100vw;
				height: 100vh;
			}

			#logo {
				position: fixed;
				top: var(--uui-size-space-5);
				left: var(--uui-size-space-5);
				height: 30px;
			}

			#logo img {
				height: 100%;
			}

			#container {
				position: relative;
				display: flex;
				align-items: center;
				justify-content: center;
				width: 100vw;
				height: 100vh;
			}

			#box {
				width: 500px;
				padding: var(--uui-size-space-6) var(--uui-size-space-5) var(--uui-size-space-5) var(--uui-size-space-5);
			}

			#email,
			#password {
				width: 100%;
			}
		`,
	];

	render() {
		return html`
			<div id="background"></div>

			<div id="logo">
				<img src="${logoImg}" alt="Umbraco" />
			</div>

			<div id="container">
				<uui-box id="box">
					<slot></slot>
				</uui-box>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-layout': UmbAuthLayoutElement;
	}
}
