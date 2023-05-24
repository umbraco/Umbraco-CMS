import { css, CSSResultGroup, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
//import logoImg from '/umbraco_logomark_white.svg';

@customElement('umb-backoffice-header')
export class UmbBackofficeHeaderElement extends UmbLitElement {
	render() {
		return html`
			<div id="appHeader">
				<uui-button id="logo" look="primary" label="Umbraco" compact>
					<img src="" alt="Umbraco" />
				</uui-button>

				<umb-backoffice-header-sections id="sections"></umb-backoffice-header-sections>
				<umb-backoffice-header-apps></umb-backoffice-header-apps>
			</div>
		`;
	}

	static styles: CSSResultGroup = [
		css`
			:host {
				width: 100%;
			}

			#appHeader {
				background-color: var(--uui-color-header-surface);
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: 0 var(--uui-size-space-5);
			}

			#logo {
				--uui-button-padding-top-factor: 1;
				--uui-button-padding-bottom-factor: 0.5;
				margin-right: var(--uui-size-space-2);
				--uui-button-background-color: transparent;
			}

			#logo img {
				height: var(--uui-size-10);
				width: var(--uui-size-10);
			}

			#sections {
				flex: 1 1 auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header': UmbBackofficeHeaderElement;
	}
}
