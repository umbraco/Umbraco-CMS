import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-logviewer-root-workspace')
export class UmbLogViewerRootWorkspaceElement extends LitElement {
	static styles = [
		css`
			:host {
				display: block;
			}

			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
		`,
	];
	render() {
		return html`
			<umb-workspace-layout headline="Log Overview for today" alias="Umb.Workspace.LogviewerRoot">
				hello
			</umb-workspace-layout>
		`;
	}
}

export default UmbLogViewerRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-root-workspace': UmbLogViewerRootWorkspaceElement;
	}
}
