import { UmbPickerSearchResultItemElementBase } from '../picker-search-result-item-element-base.js';
import { getItemFallbackIcon, getItemFallbackName } from '@umbraco-cms/backoffice/entity-item';
import { css, customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

@customElement('umb-default-picker-search-result-item')
export class UmbDefaultPickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbSearchResultItemModel> {
	override render() {
		const item = this.item;
		if (!item) return nothing;
		return html`
			<umb-ref-item
				name=${item.name ?? getItemFallbackName(item)}
				id=${item.unique}
				icon=${item.icon ?? getItemFallbackIcon()}
				select-only
				?selectable=${!this.disabled}
				?selected=${this._isSelected}
				?disabled=${this.disabled}
				@deselected=${() => this.pickerContext?.selection.deselect(item.unique)}
				@selected=${() => this.pickerContext?.selection.select(item.unique)}>
			</umb-ref-item>
		`;
	}

	static override styles = [
		css`
			umb-ref-item {
				padding-top: 0;
				padding-bottom: 0;
			}
		`,
	];
}

export { UmbDefaultPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-picker-search-result-item': UmbDefaultPickerSearchResultItemElement;
	}
}
