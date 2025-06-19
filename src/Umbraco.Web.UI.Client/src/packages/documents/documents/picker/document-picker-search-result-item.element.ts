import type { UmbDocumentSearchItemModel } from '../search/types.js';
import type { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPickerSearchResultItemElementBase } from '@umbraco-cms/backoffice/picker';

@customElement('umb-document-picker-search-result-item')
export class UmbDocumentPickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbDocumentSearchItemModel> {
	#onSelected(event: UmbSelectedEvent) {
		if (event.unique !== this.item!.unique) return;
		this.pickerContext?.selection.select(this.item!.unique);
	}

	#onDeselected(event: UmbDeselectedEvent) {
		if (event.unique !== this.item!.unique) return;
		this.pickerContext?.selection.deselect(this.item!.unique);
	}

	override render() {
		if (!this.item) return nothing;
		return html`
			<umb-entity-item-ref
				.item=${this.item}
				select-only
				?selectable=${!this.disabled}
				?selected=${this._isSelected}
				?disabled=${this.disabled}
				@deselected=${this.#onDeselected}
				@selected=${this.#onSelected}></umb-entity-item-ref>
		`;
	}
}

export { UmbDocumentPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-picker-search-result-item': UmbDocumentPickerSearchResultItemElement;
	}
}
