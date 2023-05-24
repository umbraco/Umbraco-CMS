import { css, CSSResultGroup, html, LitElement, unsafeCSS, customElement } from '@umbraco-cms/backoffice/external/lit';
import logoImg from '/umbraco_logomark_white.svg';
import installerImg from '/installer.jpg';

@customElement('umb-installer-layout')
export class UmbInstallerLayoutElement extends LitElement {
	render() {
		return html`<div>
			<div id="background" aria-hidden="true"></div>

			<div id="logo" aria-hidden="true">
				<img src="${logoImg}" alt="Umbraco" />
			</div>

			<main id="container">
				<div id="box">
					<slot></slot>
				</div>
			</main>
		</div>`;
	}

	static styles: CSSResultGroup = [
		css`
			#background {
				position: fixed;
				overflow: hidden;
				background-position: 50%;
				background-repeat: no-repeat;
				background-size: cover;
				background-image: url('${unsafeCSS(installerImg)}');
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
				justify-content: flex-end;
				width: 100%;
				min-height: 100vh;
			}

			#box {
				max-width: 500px;
				width: 100%;
				box-sizing: border-box;
				border-radius: 30px 0 0 30px;
				background-color: var(--uui-color-surface);
				display: flex;
				flex-direction: column;
				padding: var(--uui-size-layout-4) var(--uui-size-layout-4) var(--uui-size-layout-2) var(--uui-size-layout-4);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-layout': UmbInstallerLayoutElement;
	}
}
