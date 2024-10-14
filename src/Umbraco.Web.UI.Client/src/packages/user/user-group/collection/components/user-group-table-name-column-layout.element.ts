import { UMB_USER_GROUP_WORKSPACE_PATH } from '../../paths.js';
import { css, html, LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-group-table-name-column-layout')
export class UmbUserGroupTableNameColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: any;

	override render() {
		const href = UMB_USER_GROUP_WORKSPACE_PATH + '/edit/' + this.item.id;
		return html`<a href=${href}>${this.value.name}</a>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			a {
				font-weight: bold;
			}
		`,
	];
}

export default UmbUserGroupTableNameColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-table-name-column-layout': UmbUserGroupTableNameColumnLayoutElement;
	}
}
