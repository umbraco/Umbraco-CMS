import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-backoffice-header-apps')
export class UmbBackofficeHeaderAppsElement extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#apps {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
		`,
	];

	render() {
		return html` <umb-extension-slot id="apps" type="headerApp"></umb-extension-slot> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-apps': UmbBackofficeHeaderAppsElement;
	}
}
