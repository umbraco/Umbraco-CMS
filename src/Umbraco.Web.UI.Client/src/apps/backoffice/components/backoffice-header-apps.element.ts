import { css, CSSResultGroup, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-backoffice-header-apps')
export class UmbBackofficeHeaderAppsElement extends LitElement {
	render() {
		return html` <umb-extension-slot id="apps" type="headerApp"></umb-extension-slot> `;
	}

	static styles: CSSResultGroup = [
		css`
			#apps {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-apps': UmbBackofficeHeaderAppsElement;
	}
}
