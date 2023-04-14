import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-media-edit-workspace-view')
export class UmbMediaEditWorkspaceViewElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`<div>Render Media Props</div>`;
	}
}

export default UmbMediaEditWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-edit-workspace-view': UmbMediaEditWorkspaceViewElement;
	}
}
