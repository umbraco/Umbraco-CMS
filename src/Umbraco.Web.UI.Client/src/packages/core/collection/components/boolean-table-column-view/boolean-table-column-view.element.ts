import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-boolean-table-column-view';
@customElement(elementName)
export class UmbBooleanTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	value = false;

	override render() {
		return this.value ? html`<uui-icon name="icon-check"></uui-icon>` : nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbBooleanTableColumnViewElement;
	}
}
