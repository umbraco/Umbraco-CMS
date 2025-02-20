import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-backoffice-header')
export class UmbBackofficeHeaderElement extends UmbLitElement {
	override render() {
		return html`
			<div id="appHeader">
				<umb-backoffice-header-logo></umb-backoffice-header-logo>
				<umb-backoffice-header-sections id="sections"></umb-backoffice-header-sections>
				<umb-backoffice-header-apps></umb-backoffice-header-apps>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				width: 100%;
			}

			#appHeader {
				background-color: var(--umb-header-background-color, var(--uui-color-header-surface));
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: 0 var(--uui-size-space-5);
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
