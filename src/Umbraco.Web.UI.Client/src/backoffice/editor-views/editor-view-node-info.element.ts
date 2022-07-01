import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-editor-view-node-info')
export class UmbEditorViewNodeInfo extends LitElement {
	static styles = [UUITextStyles, css``];

	@property()
	node: any;

	render() {
		return html`<div>Info Editor View</div>`;
	}
}

export default UmbEditorViewNodeInfo;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-node-info': UmbEditorViewNodeInfo;
	}
}
