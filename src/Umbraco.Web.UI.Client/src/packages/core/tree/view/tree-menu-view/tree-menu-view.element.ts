import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-menu-view')
export class UmbTreeMenuViewElement extends UmbLitElement {
	override render() {
		return html`<div>Tree Menu View Element</div>`;
	}
}

export { UmbTreeMenuViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-menu-view': UmbTreeMenuViewElement;
	}
}
