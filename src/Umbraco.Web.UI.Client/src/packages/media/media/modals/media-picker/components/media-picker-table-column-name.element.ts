import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

export interface UmbMediaPickerTableColumnNameValue {
	name: string;
	ancestorPath?: string;
	/** When set, the name renders as a button that invokes this on click (e.g. to open a folder). */
	navigate?: () => void;
}

@customElement('umb-media-picker-table-column-name')
export class UmbMediaPickerTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbMediaPickerTableColumnNameValue;

	#onClick(event: Event) {
		event.stopPropagation();
		this.value?.navigate?.();
	}

	override render() {
		if (!this.value) return nothing;
		return html`
			${this.value.navigate
				? html`<uui-button look="default" compact label=${this.value.name} @click=${this.#onClick}
						>${this.value.name}</uui-button
					>`
				: html`<span class="name">${this.value.name}</span>`}
			${this.value.ancestorPath
				? html`<span class="ancestor-path">${this.value.ancestorPath}</span>`
				: nothing}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			uui-button {
				--uui-button-padding-left-factor: 0;
				text-align: left;
			}

			.name {
				display: block;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}

			.ancestor-path {
				display: block;
				font-size: 0.8em;
				opacity: 0.6;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
				/* Truncate from the start so the deepest (most disambiguating) ancestor stays visible. */
				direction: rtl;
				text-align: left;
			}
		`,
	];
}

export default UmbMediaPickerTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-table-column-name': UmbMediaPickerTableColumnNameElement;
	}
}
