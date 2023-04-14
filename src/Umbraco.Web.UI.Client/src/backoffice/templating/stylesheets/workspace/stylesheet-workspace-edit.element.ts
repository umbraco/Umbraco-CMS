import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-stylesheet-workspace-edit')
export class UmbStylesheetWorkspaceEditElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	render() {
		return html` <umb-workspace-layout alias="Umb.Workspace.Stylesheet">Stylesheet workspace</umb-workspace-layout> `;
	}
}

export default UmbStylesheetWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-edit': UmbStylesheetWorkspaceEditElement;
	}
}
