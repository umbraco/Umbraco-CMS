import {css, CSSResultGroup, html, LitElement, nothing} from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { styleMap } from 'lit/directives/style-map.js';

@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
	@property({ attribute: 'background-image' })
	backgroundImage?: string;

	@property({ attribute: 'logo-image' })
	logoImage?: string;

  constructor() {
    super();

    if ((window as any).Umbraco) {
      this.backgroundImage = this.backgroundImage || (window as any).Umbraco.Sys.ServerVariables.umbracoSettings.loginBackgroundImage;
      this.logoImage = this.logoImage || (window as any).Umbraco.Sys.ServerVariables.umbracoSettings.loginLogoImage;
    }
  }

	render() {
		return html`
			<div id="background" style=${styleMap({ backgroundImage: `url('${this.backgroundImage}')` })}></div>

			${this.logoImage ? html`<div id="logo"><img src=${this.logoImage} alt="Umbraco" /></div>` : nothing}

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
