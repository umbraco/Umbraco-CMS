import type { UmbElementSearchItemModel } from '../search/types.js';
import type { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPickerSearchResultItemElementBase } from '@umbraco-cms/backoffice/picker';

@customElement('umb-element-picker-search-result-item')
export class UmbElementPickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbElementSearchItemModel> {
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
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}></umb-entity-item-ref>
		`;
	}
}

export { UmbElementPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-picker-search-result-item': UmbElementPickerSearchResultItemElement;
	}
}
