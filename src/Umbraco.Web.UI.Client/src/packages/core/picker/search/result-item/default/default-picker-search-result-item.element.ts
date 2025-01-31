import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPickerContext } from '@umbraco-cms/backoffice/picker';
import { UMB_PICKER_CONTEXT } from '@umbraco-cms/backoffice/picker';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

const elementName = 'umb-default-picker-search-result-item';
@customElement(elementName)
export class UmbDefaultPickerSearchResultItemElement extends UmbLitElement {
	#item: UmbSearchResultItemModel | undefined;
	@property({ type: Object })
	public get item(): UmbSearchResultItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbSearchResultItemModel | undefined) {
		this.#item = value;
		this.#observeIsSelected();
	}

	@state()
	_isSelected = false;

	#pickerContext?: UmbPickerContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_CONTEXT, (context) => {
			this.#pickerContext = context;
			this.#observeIsSelected();
		});
	}

	#observeIsSelected() {
		const selectionManager = this.#pickerContext?.selection;
		if (!selectionManager) return;

		const unique = this.item?.unique;
		if (unique === undefined) return;

		this.observe(selectionManager.selection, () => {
			this._isSelected = selectionManager.isSelected(unique);
		});
	}

	override render() {
		const item = this.item;
		if (!item) return nothing;

		return html`
			<umb-ref-item
				name=${item.name}
				id=${item.unique}
				icon=${item.icon ?? 'icon-document'}
				select-only
				selectable
				@selected=${() => this.#pickerContext?.selection.select(item.unique)}
				@deselected=${() => this.#pickerContext?.selection.deselect(item.unique)}
				?selected=${this._isSelected}>
			</umb-ref-item>
		`;
	}

	static override readonly styles = [UmbTextStyles];
}

export { UmbDefaultPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDefaultPickerSearchResultItemElement;
	}
}
