import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../../../shared/components/workspace/workspace-entity/workspace-entity.element';

@customElement('umb-media-type-workspace')
export class UmbMediaTypeWorkspaceElement extends LitElement {
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

	@property()
	id!: string;

	render() {
		return html`<umb-workspace-entity alias="Umb.Workspace.MediaType">Media Type Workspace</umb-workspace-entity>`;
	}
}

export default UmbMediaTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-': UmbMediaTypeWorkspaceElement;
	}
}
