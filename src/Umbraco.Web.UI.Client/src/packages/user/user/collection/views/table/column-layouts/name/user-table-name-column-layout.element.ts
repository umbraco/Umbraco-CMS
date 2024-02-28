import { html, LitElement, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
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
		const avatarUrls = [
			{
				scale: '1x',
				url: this.value.avatarUrls?.[0],
			},
			{
				scale: '2x',
				url: this.value.avatarUrls?.[1],
			},
			{
				scale: '3x',
				url: this.value.avatarUrls?.[2],
			},
		];

		let avatarSrcset = '';

		avatarUrls.forEach((url) => {
			avatarSrcset += `${url.url} ${url.scale},`;
		});

		return html` <div style="display: flex; align-items: center;">
			<uui-avatar
				style="margin-right: var(--uui-size-space-3);"
				.name=${this.value.name || 'Unknown'}
				img-src=${ifDefined(this.value.avatarUrls.length > 0 ? avatarUrls[0].url : undefined)}
				img-srcset=${ifDefined(this.value.avatarUrls.length > 0 ? avatarSrcset : undefined)}></uui-avatar>
			<a style="font-weight: bold;" href="section/user-management/view/users/user/${this.value.unique}"
				>${this.value.name}</a
			>
		</div>`;
	}
}

export default UmbUserTableNameColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-table-name-column-layout': UmbUserTableNameColumnLayoutElement;
	}
}
