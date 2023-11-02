import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { styleMap } from 'lit/directives/style-map.js';
import { when } from 'lit/directives/when.js';

@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
	@property({ attribute: 'background-image' })
	backgroundImage?: string;

	@property({ attribute: 'logo-light' })
	logoLight?: string;

	@property({ attribute: 'logo-dark' })
	logoDark?: string;

	#renderLogo() {
		return html`<div
			style=${styleMap({
				'--logo-light': `url(${this.logoLight})`,
				'--logo-dark': `url(${this.logoDark})`,
			})}
			id="logo"></div>`;
	}

	#renderImage() {
		this.toggleAttribute('has-background-image', this.backgroundImage ? true : false);
		this.style.setProperty('--background-image', `url(${this.backgroundImage})`);

		return html`<div id="image-column">
			<div id="image" style=${styleMap({ backgroundImage: `url(${this.backgroundImage})` })}></div>
		</div>`;
	}

	render() {
		return html`
			<div id="layout">
				${this.#renderLogo()} ${when(this.backgroundImage, () => this.#renderImage())}
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
				position: relative;
				z-index: 0;
			}
			:host::before {
				background-image: var(--background-image);
				background-position: 50%;
				background-repeat: no-repeat;
				background-size: cover;
				content: '';
				inset: 0;
				position: absolute;
				width: 100%;
				height: 100%;
				z-index: -2;
			}
			:host::after {
				content: '';
				position: absolute;
				inset: 0;
				width: calc(100% - 32px);
				height: calc(100% - 32px);
				margin: auto;
				background-color: #fff;
				opacity: 0.95;
				border-radius: 40px;
				z-index: -1;
			}
			#layout {
				display: flex;
				height: 100%;
			}
			:host([has-background-image]) #layout {
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
				background-image: var(--logo-dark);
				background-repeat: no-repeat;
				z-index: 1;
				top: 52px;
				left: 52px;
				width: 120px;
				height: 39px;
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
				width: 100%;
				height: 100%;
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
			@media (min-width: 500px) {
				:host::after {
					width: calc(100% - 64px);
					height: calc(100% - 64px);
				}
			}
			@media (min-width: 979px) {
				:host::before {
					background-image: none;
				}
				:host([has-background-image]) #image-column {
					display: block;
				}
				:host([has-background-image]) #auth-box {
					width: 300px;
				}
				:host([has-background-image]) #logo {
					background-image: var(--logo-light);
					top: 52px;
					left: 52px;
					width: 140px;
					height: 39px;
				}
				:host([has-background-image]) #auth-column {
					max-height: calc(50% + 200px);
					width: max-content;
					padding-inline: 100px;
				}
			}
			@media (min-width: 1200px) {
				:host([has-background-image]) #image-column {
					padding: var(--uui-size-layout-3);
					padding-right: 0;
				}
				:host([has-background-image]) #auth-column {
					padding-inline: 200px;
				}
				:host([has-background-image]) #logo {
					top: 68px;
					left: 68px;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-layout': UmbAuthLayoutElement;
	}
}
