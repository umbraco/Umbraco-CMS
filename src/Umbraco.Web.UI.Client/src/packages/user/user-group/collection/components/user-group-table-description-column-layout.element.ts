import { css, html, LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-user-group-table-description-column-layout')
export class UmbUserGroupTableDescriptionColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: string;

	override render() {
		if (!this.value) {
			return html``;
		}
		return html`<span title=${this.value}>${this.value}</span>`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				max-width: 300px;
			}

			span {
				display: block;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}
		`,
	];
}

export default UmbUserGroupTableDescriptionColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-table-description-column-layout': UmbUserGroupTableDescriptionColumnLayoutElement;
	}
}
