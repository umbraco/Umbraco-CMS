import type { UmbDocumentTypeSearchItemModel } from '../search/types.js';
import { customElement, html, nothing, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbPickerSearchResultItemElementBase } from '@umbraco-cms/backoffice/picker';

@customElement('umb-document-type-picker-search-result-item')
export class UmbDocumentTypePickerSearchResultItemElement extends UmbPickerSearchResultItemElementBase<UmbDocumentTypeSearchItemModel> {
	override render() {
		if (!this.item) return nothing;
		const item = this.item;
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
				${when(
					item.isElement,
					() => html`
						<uui-tag slot="tag" look="secondary">
							<umb-localize key="contentTypeEditor_elementType">Element Type</umb-localize>
						</uui-tag>
					`,
				)}
			</umb-ref-item>
		`;
	}
}

export { UmbDocumentTypePickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-picker-search-result-item': UmbDocumentTypePickerSearchResultItemElement;
	}
}
