import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-workspace-package-builder')
export class UmbWorkspacePackageBuilderElement extends LitElement {
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
		return html`<umb-workspace-entity-layout alias="Umb.Workspace.PackageBuilder"
			>PACKAGE BUILDER</umb-workspace-entity-layout
		> `;
	}
}

export default UmbWorkspacePackageBuilderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-package-builder': UmbWorkspacePackageBuilderElement;
	}
}
