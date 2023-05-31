import { html, LitElement , customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-user-table-name-column-layout')
export class UmbUserTableNameColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: any;

	render() {
		return html` <div style="display: flex; align-items: center;">
			<uui-avatar name="${this.value.name}" style="margin-right: var(--uui-size-space-3);"></uui-avatar>
			<a style="font-weight: bold;" href="section/users/view/users/user/${this.item.id}">${this.value.name}</a>
		</div>`;
	}
}

export default UmbUserTableNameColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-table-name-column-layout': UmbUserTableNameColumnLayoutElement;
	}
}
