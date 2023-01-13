import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-language-root-workspace')
export class UmbLanguageRootWorkspaceElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			umb-table {
				padding: 0;
				margin: var(--uui-size-space-3) var(--uui-size-space-6);
			}
			#add-language {
				margin-left: var(--uui-size-space-6);
			}
		`,
	];

	render() {
		return html`
			<umb-body-layout no-header-background>
				<uui-button id="add-language" slot="header" label="Add language" look="outline" color="default"></uui-button>
				<!-- <div slot="header" id="toolbar">
				</div> -->
				<umb-table></umb-table>
			</umb-body-layout>
		`;
	}
}

export default UmbLanguageRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-workspace': UmbLanguageRootWorkspaceElement;
	}
}
