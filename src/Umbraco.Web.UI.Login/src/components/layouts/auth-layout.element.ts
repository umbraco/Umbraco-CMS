import { css, CSSResultGroup, html, LitElement, nothing, PropertyValueMap } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';

@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
	@property({ attribute: 'background-image' })
	backgroundImage?: string;

	@property({ attribute: 'logo-image' })
	logoImage?: string;

	@property({ attribute: 'logo-image-alternative' })
	logoImageAlternative?: string;

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has<keyof this>('backgroundImage')) {
			this.style.setProperty('--logo-alternative-display', this.backgroundImage ? 'none' : 'unset');
			this.style.setProperty('--image', `url('${this.backgroundImage}')`);
		}
	}

	#renderImageContainer() {
		if (!this.backgroundImage) return nothing;

		return html`
			<div id="image-container">
				<div id="image">
					<svg
						id="curve-top"
						width="1746"
						height="1374"
						viewBox="0 0 1746 1374"
						fill="none"
						xmlns="http://www.w3.org/2000/svg">
						<path d="M8 1C61.5 722.5 206.5 1366.5 1745.5 1366.5" stroke="#F5C1BC" stroke-width="15" />
					</svg>
					<svg
						id="curve-bottom"
						width="1364"
						height="552"
						viewBox="0 0 1364 552"
						fill="none"
						xmlns="http://www.w3.org/2000/svg">
						<path d="M1 8C387 24 1109 11 1357 548" stroke="#F5C1BC" stroke-width="15" />
					</svg>

					${when(
						this.logoImage,
						() => html`<img id="logo-on-image" src=${this.logoImage!} alt="logo" aria-hidden="true" />`
					)}
				</div>
			</div>
		`;
	}

	#renderContent() {
		return html`
			<div id="content-container">
				<div id="content">
					<slot></slot>
				</div>
			</div>
		`;
	}

	render() {
		return html`
			<div id=${this.backgroundImage ? 'main' : 'main-no-image'}>
				${this.#renderImageContainer()} ${this.#renderContent()}
			</div>
			${when(
				this.logoImageAlternative,
				() => html`<img id="logo-on-background" src=${this.logoImageAlternative!} alt="logo" aria-hidden="true" />`
			)}
		`;
	}

	static styles: CSSResultGroup = [
		css`
			:host {
				--uui-color-interactive: #283a97;
				--uui-button-border-radius: 45px;
				--uui-color-default: var(--uui-color-interactive);
				--uui-button-height: 42px;

				--input-height: 40px;
				--header-font-size: 3rem;
				--header-secondary-font-size: 2.4rem;
			}
			#main-no-image,
			#main {
				max-width: 1920px;
				display: flex;
				height: 100vh;
				padding: 8px;
				box-sizing: border-box;
				margin: 0 auto;
			}
			#image-container {
				display: none;
				width: 100%;
			}
			#content-container {
				display: flex;
				width: 100%;
				box-sizing: border-box;
				overflow: auto;
			}
			#content {
				max-width: 340px;
				margin: auto;
				width: 100%;
			}
			#image {
				background-image: var(--image);
				background-position: 50%;
				background-repeat: no-repeat;
				background-size: cover;
				width: 100%;
				height: 100%;
				border-radius: 38px;
				position: relative;
				overflow: hidden;
			}
			#image svg {
				position: absolute;
				width: 45%;
				height: fit-content;
			}
			#curve-top {
				top: 0;
				right: 0;
			}
			#curve-bottom {
				bottom: 0;
				left: 0;
			}
			#logo-on-image {
				position: absolute;
				top: 24px;
				left: 24px;
				height: 30px;
			}
			#logo-on-background {
				position: fixed;
				top: 24px;
				left: 24px;
				height: 30px;
			}
			@media only screen and (min-width: 900px) {
				:host {
					--header-font-size: 4rem;
				}
				#main {
					padding: 32px;
					padding-right: 0;
				}
				#image-container {
					display: block;
				}
				#content-container {
					display: flex;
					padding: 16px;
				}
				#logo-on-background {
					display: var(--logo-alternative-display);
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
