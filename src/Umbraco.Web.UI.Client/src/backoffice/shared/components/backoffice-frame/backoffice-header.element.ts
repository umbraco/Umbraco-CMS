import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement, unsafeCSS } from 'lit';
import { customElement } from 'lit/decorators.js';
import logoImg from '/umbraco_logomark_white.svg';

import './backoffice-header-sections.element';
import './backoffice-header-apps.element';

@customElement('umb-backoffice-header')
export class UmbBackofficeHeaderElement extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
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

	render() {
		return html`
			<div id="appHeader">
				<uui-button id="logo" look="primary" label="Umbraco" compact>
					<img src="${logoImg}" alt="Umbraco" />
				</uui-button>

				<umb-backoffice-header-sections id="sections"></umb-backoffice-header-sections>
				<umb-backoffice-header-apps></umb-backoffice-header-apps>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header': UmbBackofficeHeaderElement;
	}
}
