import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-workspace-package')
export class UmbWorkspacePackageElement extends LitElement {
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
		return html`<umb-workspace-entity alias="Umb.Workspace.Package">PACKAGE Workspace</umb-workspace-entity> `;
	}
}

export default UmbWorkspacePackageElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-package': UmbWorkspacePackageElement;
	}
}
