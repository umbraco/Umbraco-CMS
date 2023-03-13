import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-section-view')
export class UmbSectionViewElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`<div>Render Section View</div></div>`;
	}
}

export default UmbSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view': UmbSectionViewElement;
	}
}
