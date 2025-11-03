import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-card-view')
export class UmbTreeCardViewElement extends UmbLitElement {
	override render() {
		return html`<div>Tree Card View Element</div>`;
	}
}

export { UmbTreeCardViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-card-view': UmbTreeCardViewElement;
	}
}
