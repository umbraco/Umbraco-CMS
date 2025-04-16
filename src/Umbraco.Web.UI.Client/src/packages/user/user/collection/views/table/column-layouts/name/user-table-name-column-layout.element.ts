import { UMB_USER_WORKSPACE_PATH } from '../../../../../paths.js';
import { html, LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-table-name-column-layout')
export class UmbUserTableNameColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: any;

	override render() {
		const href = UMB_USER_WORKSPACE_PATH + '/edit/' + this.value.unique;

		return html` <div style="display: flex; align-items: center;">
			<umb-user-avatar
				style="margin-right: var(--uui-size-space-3);"
				name=${this.value.name}
				kind=${this.value.kind}
				.imgUrls=${this.value.avatarUrls}></umb-user-avatar>
			<a style="font-weight: bold;" href="${href}">${this.value.name}</a>
		</div>`;
	}
	static override styles = [UmbTextStyles];
}

export default UmbUserTableNameColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-table-name-column-layout': UmbUserTableNameColumnLayoutElement;
	}
}
