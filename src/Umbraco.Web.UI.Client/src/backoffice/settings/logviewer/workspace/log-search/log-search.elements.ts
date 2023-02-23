import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/router';

@customElement('umb-log-search-workspace-search')
export class UmbLogSearchWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}

			#header {
				display: flex;
				align-items: center;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
		`,
	];

	private _routerPath?: string;

	render() {
		return html` <h1>Search</h1> `;
	}
}

export default UmbLogSearchWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-search-workspace': UmbLogSearchWorkspaceElement;
	}
}
