import type { UmbDocumentSearchItemModel } from '../search/types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPickerSearchResultItemElementBase } from '@umbraco-cms/backoffice/picker';

@customElement('umb-document-picker-search-result-item')
export class UmbDocumentPickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbDocumentSearchItemModel> {
	override render() {
		if (!this.item) return nothing;
		const item = this.item;
		return html`
			<umb-ref-item
				name=${item.name}
				id=${item.unique}
				icon=${item.documentType.icon ?? 'icon-document'}
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

export { UmbDocumentPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-picker-search-result-item': UmbDocumentPickerSearchResultItemElement;
	}
}
