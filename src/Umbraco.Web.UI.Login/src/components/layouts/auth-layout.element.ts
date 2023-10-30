import { css, CSSResultGroup, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { styleMap } from 'lit/directives/style-map.js';
import { when } from 'lit/directives/when.js';

@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
	@property({ attribute: 'background-image' })
	backgroundImage?: string; //TODO: Implement

	@property({ attribute: 'logo-light' })
	logoLight?: string;

	@property({ attribute: 'logo-dark' })
	logoDark?: string;

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('logoImage')) {
			this.style.setProperty('--logo-light', `url(${this.logoLight})`);
			this.style.setProperty('--logo-dark', `url(${this.logoDark})`);
		}
	}

	#renderImage() {
		return html` <div id="image-column">
			<div id="image" style=${styleMap({ backgroundImage: `url(${this.backgroundImage})` })}></div>
		</div>`;
	}

	render() {
		return html`
			<div id="layout">
				<div id="logo"></div>
				${when(this.backgroundImage, () => this.#renderImage())}
				<div id="auth-column">
					<div id="auth-box">
						<slot></slot>
					</div>
				</div>
			</div>
		`;
	}

	static styles: CSSResultGroup = [
		css`
			:host {
				display: block;
				height: 100dvh;
				background-color: var(--uui-color-surface);
			}
			#layout {
				display: flex;
				height: 100%;
				max-width: 2000px;
			}
			#image-column {
				display: none;
				padding: var(--uui-size-layout-2);
				width: 100%;
				overflow: hidden;
				padding-right: 0;
				box-sizing: border-box;
				overflow: visible;
				padding-right: 0;
			}
			#logo {
				position: fixed;
				background-image: url('login/umbraco_logo_blue_horizontal.svg'); /* should use a dark logo here */
				background-repeat: no-repeat;
				z-index: 1;
				width: 80px;
				height: 22px;
				top: var(--uui-size-space-4);
				left: var(--uui-size-space-4);
			}
			#auth-column {
				display: flex;
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				margin-block: auto;
				padding-block: var(--uui-size-layout-2);
				padding-inline: var(--uui-size-space-4);
				max-height: calc(50% + 100px);
			}
			#image {
				z-index: 0;
				overflow: hidden;
				background-position: 50%;
				background-repeat: no-repeat;
				background-size: cover;
				background-image: url('login/loginImage.jpg');
				width: 100%;
				height: 100%;
				transform: scaleX(-1);
				border-radius: 40px;
				/* border: 1px solid white; */
				box-sizing: border-box;
				/* box-shadow: rgba(149, 157, 165, 0.2) 0px 8px 48px; */
			}
			#auth-box {
				max-width: 300px;
				width: 100%;
				box-sizing: border-box;
				margin-inline: auto;
			}
			@media (min-width: 850px) {
				#image-column {
					display: block;
				}
				#auth-box {
					width: 300px;
				}
				#logo {
					background-image: url('login/umbraco_logo_white_horizontal.svg');
					top: 48px;
					left: 48px;
					width: 140px;
					height: 39px;
				}
				#auth-column {
					max-height: calc(50% + 200px);
					width: max-content;
					padding-inline: 100px;
				}
			}
			@media (min-width: 1300px) {
				#image-column {
					padding: var(--uui-size-layout-3);
					padding-right: 0;
				}
				#auth-column {
					padding-inline: 200px;
				}
				#logo {
					top: 64px;
					left: 64px;
				}
			}
			@media (min-width: 1600px) {
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-layout': UmbAuthLayoutElement;
	}
}
