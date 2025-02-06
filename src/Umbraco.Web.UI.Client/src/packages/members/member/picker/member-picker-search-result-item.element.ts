import type { UmbMemberSearchItemModel } from '../search/types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPickerSearchResultItemElementBase } from '@umbraco-cms/backoffice/picker';

@customElement('umb-member-picker-search-result-item')
export class UmbMemberPickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbMemberSearchItemModel> {
	override render() {
		if (!this.item) return nothing;
		const item = this.item;
		return html`
			<umb-ref-item
				name=${item.name}
				id=${item.unique}
				icon=${item.memberType.icon ?? 'icon-user'}
				select-only
				selectable
				?selected=${this._isSelected}
				@deselected=${() => this.pickerContext?.selection.deselect(item.unique)}
				@selected=${() => this.pickerContext?.selection.select(item.unique)}>
			</umb-ref-item>
		`;
	}
}

export { UmbMemberPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-search-result-item': UmbMemberPickerSearchResultItemElement;
	}
}
