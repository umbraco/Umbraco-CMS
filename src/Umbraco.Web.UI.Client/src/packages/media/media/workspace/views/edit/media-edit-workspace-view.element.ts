import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-media-edit-workspace-view')
export class UmbMediaEditWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceEditorViewExtensionElement {
	render() {
		return html`<div>Render Media Props</div>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbMediaEditWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-edit-workspace-view': UmbMediaEditWorkspaceViewElement;
	}
}
