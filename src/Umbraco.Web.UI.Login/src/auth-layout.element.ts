import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { styleMap } from 'lit-html/directives/style-map.js';
import {UmbIconRegistry} from "./icon.registry.ts";
import {UUIIconRegistryEssential} from "@umbraco-ui/uui";

@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
	@property()
	backgroundImage = 'login/login.svg';

	@property()
	logoImage = 'login/umbraco_logomark_white.svg';

  constructor() {
    super();

    new UUIIconRegistryEssential().attach(this);
    new UmbIconRegistry().attach(this);
  }

	render() {
		return html`
			<div id="background" style=${styleMap({ backgroundImage: `url('${this.backgroundImage}')` })}></div>

			<div id="logo"><img src=${this.logoImage} alt="Umbraco" /></div>

			<div id="container">
				<div id="box">
					<slot></slot>
				</div>
			</div>
		`;
	}

	static styles: CSSResultGroup = [
		css`
			#background {
				position: fixed;
				overflow: hidden;
				background-position: 50%;
				background-repeat: no-repeat;
				background-size: cover;
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
				padding: var(--uui-size-layout-3);
				background-color: var(--uui-color-surface-alt);
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-5);
				border-radius: calc(var(--uui-border-radius) * 2);
			}

			#email,
			#password {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-layout': UmbAuthLayoutElement;
	}
}
