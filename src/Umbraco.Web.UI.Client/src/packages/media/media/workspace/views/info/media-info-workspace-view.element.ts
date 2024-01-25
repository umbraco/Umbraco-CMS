import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-media-info-workspace-view')
export class UmbMediaInfoWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	render() {
		return html`<div>Media info</div>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbMediaInfoWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-info-workspace-view': UmbMediaInfoWorkspaceViewElement;
	}
}
