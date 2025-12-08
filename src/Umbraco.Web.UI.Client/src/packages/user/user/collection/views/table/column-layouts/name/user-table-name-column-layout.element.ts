import { html, LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn } from '@umbraco-cms/backoffice/components';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-table-name-column-layout')
export class UmbUserTableNameColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ attribute: false })
	value!: {
		name: string;
		unique: string;
		kind: string;
		avatarUrls: Record<string, string>;
		href?: string;
	};

	override render() {
		return html` <div style="display: flex; align-items: center;">
			<umb-user-avatar
				style="margin-right: var(--uui-size-space-3);"
				name=${this.value.name}
				kind=${this.value.kind}
				.imgUrls=${this.value.avatarUrls}></umb-user-avatar>

			${this.value.href
				? html`<a style="font-weight: bold;" href="${this.value.href}">${this.value.name}</a>`
				: html` <span>${this.value.name}</span>`}
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
