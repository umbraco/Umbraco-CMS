import { UmbPickerSearchResultItemElementBase } from '../picker-search-result-item-element-base.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

@customElement('umb-default-picker-search-result-item')
export class UmbDefaultPickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbSearchResultItemModel> {
	override render() {
		const item = this.item;
		if (!item) return nothing;
		return html`
			<umb-ref-item
				name=${item.name}
				id=${item.unique}
				icon=${item.icon ?? 'icon-document'}
				select-only
				?selectable=${!this.disabled}
				?selected=${this._isSelected}
				?disabled=${this.disabled}
				@deselected=${() => this.pickerContext?.selection.deselect(item.unique)}
				@selected=${() => this.pickerContext?.selection.select(item.unique)}>
			</umb-ref-item>
		`;
	}
}

export { UmbDefaultPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-picker-search-result-item': UmbDefaultPickerSearchResultItemElement;
	}
}
