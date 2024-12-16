import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-installer-layout')
export class UmbInstallerLayoutElement extends LitElement {
	override render() {
		return html`
			<div id="logo">
				<umb-app-logo></umb-app-logo>
			</div>

			<main id="container">
				<div id="grid">
					<div id="illustration"></div>
					<div id="box">
						<slot id="slot"></slot>
					</div>
				</div>
			</main>
		`;
	}

	static override styles: CSSResultGroup = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;
				position: relative;
				height: 100%;

				background-color: hsla(240, 68%, 11%, 1);
				background-image: radial-gradient(at 99% 2%, hsla(212, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 98% 95%, hsla(255, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 1% 2%, hsla(249, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 2% 97%, hsla(228, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 74% 57%, hsla(216, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 37% 30%, hsla(205, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 26% 70%, hsla(235, 40%, 12%, 1) 0px, transparent 50%),
					radial-gradient(at 98% 48%, hsla(355, 40%, 12%, 1) 0px, transparent 50%);
			}

			#logo {
				position: fixed;
				top: var(--uui-size-space-5);
				left: var(--uui-size-space-5);
				height: 30px;
				z-index: 10;
			}

			#container {
				container: container / inline-size;
				width: 100%;
				max-width: 1200px;
				height: 100%;
				max-height: 900px;
				box-shadow:
					0px 1.1px 3.7px rgba(0, 0, 0, 0.091),
					0px 3.1px 10.1px rgba(0, 0, 0, 0.13),
					0px 7.5px 24.4px rgba(0, 0, 0, 0.169),
					0px 25px 81px rgba(0, 0, 0, 0.26);
			}

			#grid {
				display: grid;
				grid-template-columns: 1fr 1fr;
				width: 100%;
				height: 100%;
			}

			@container container (max-width: 800px) {
				#grid {
					grid-template-columns: 1fr;
				}

				#illustration {
					display: none;
				}
			}

			#box {
				overflow: auto; /*  temp fix. Scrolling should be handled by each slotted element */
				container: box / inline-size;
				box-sizing: border-box;
				width: 100%;
				height: 100%;
				display: flex;
				background-color: var(--uui-color-surface);
				flex-direction: column;
			}
			#slot {
				display: block;
				padding: var(--uui-size-layout-4) var(--uui-size-layout-4) var(--uui-size-layout-2) var(--uui-size-layout-4);
				height: 100%;
				width: 100%;
				max-width: 400px;
				margin: 0 auto;
			}

			@container box (max-width: 500px) {
				#slot {
					padding: var(--uui-size-layout-2) var(--uui-size-layout-2) var(--uui-size-layout-1) var(--uui-size-layout-2);
				}
			}

			#illustration {
				background-image: url('/umbraco/backoffice/assets/installer-illustration.svg');
				background-repeat: no-repeat;
				background-size: cover;
				position: relative;
			}
			#illustration:after {
				content: '';
				display: block;
				background: #5b84ff17;
				position: absolute;
				inset: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-layout': UmbInstallerLayoutElement;
	}
}
