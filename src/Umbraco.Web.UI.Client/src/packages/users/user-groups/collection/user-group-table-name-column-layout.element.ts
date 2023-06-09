import { html, LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-user-group-table-name-column-layout')
export class UmbUserGroupTableNameColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: any;

	render() {
		return html` <a style="font-weight: bold;" href="section/users/view/user-groups/user-group/edit/${this.item.id}">
			${this.value.name}
		</a>`;
	}
}

export default UmbUserGroupTableNameColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-table-name-column-layout': UmbUserGroupTableNameColumnLayoutElement;
	}
}
