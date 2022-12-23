import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../../../core/components/workspace/workspace-entity/workspace-entity.element';

@customElement('umb-workspace-media-type')
export class UmbWorkspaceMediaTypeElement extends LitElement {
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

export default UmbWorkspaceMediaTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-media-type': UmbWorkspaceMediaTypeElement;
	}
}
