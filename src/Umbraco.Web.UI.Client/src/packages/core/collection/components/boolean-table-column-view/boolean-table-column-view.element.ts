import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-boolean-table-column-view')
export class UmbBooleanTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	value = false;

	override render() {
		return this.value ? html`<uui-icon name="icon-check"></uui-icon>` : nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-boolean-table-column-view': UmbBooleanTableColumnViewElement;
	}
}
