import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-logviewer-root-workspace')
export class UmbLogViewerRootWorkspaceElement extends LitElement {
	render() {
		return html`
		<div>
			<h1>LogViewer Root Workspace</h1>
		</div>
		`;
	}
}

export default UmbLogViewerRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-root-workspace': UmbLogViewerRootWorkspaceElement;
	}
}
