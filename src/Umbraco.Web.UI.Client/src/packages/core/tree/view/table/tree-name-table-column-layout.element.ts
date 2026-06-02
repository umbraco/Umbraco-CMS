import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

export interface UmbTreeNameTableColumnValue {
	name: string;
	href?: string;
	onOpen?: () => void;
}

@customElement('umb-tree-name-table-column-layout')
export class UmbTreeNameTableColumnLayoutElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbTreeNameTableColumnValue;

	#onOpenClick(e: Event) {
		e.stopPropagation();
		this.value.onOpen?.();
	}

	override render() {
		if (!this.value) return nothing;

		if (this.value.href) {
			return html`<uui-button compact label=${this.value.name} href=${this.value.href}>${this.value.name}</uui-button>`;
		}

		if (this.value.onOpen) {
			return html`<uui-button compact label=${this.value.name} @click=${this.#onOpenClick}
				>${this.value.name}</uui-button
			>`;
		}

		return html`<span>${this.value.name}</span>`;
	}

	static override styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-name-table-column-layout': UmbTreeNameTableColumnLayoutElement;
	}
}
