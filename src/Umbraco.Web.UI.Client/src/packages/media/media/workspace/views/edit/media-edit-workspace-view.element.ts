import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-media-edit-workspace-view')
export class UmbMediaEditWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceEditorViewExtensionElement {
	render() {
		return html`<div>Render Media Props</div>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbMediaEditWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-edit-workspace-view': UmbMediaEditWorkspaceViewElement;
	}
}
