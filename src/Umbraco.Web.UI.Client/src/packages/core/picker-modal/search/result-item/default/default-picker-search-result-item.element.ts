import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPickerModalContext } from '@umbraco-cms/backoffice/picker-modal';
import { UMB_PICKER_MODAL_CONTEXT } from '@umbraco-cms/backoffice/picker-modal';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

const elementName = 'umb-default-picker-search-result-item';
@customElement(elementName)
export class UmbDefaultPickerSearchResultItemElement extends UmbLitElement {
	#item?: UmbSearchResultItemModel | undefined;
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

	#pickerModalContext?: UmbPickerModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_MODAL_CONTEXT, (context) => {
			this.#pickerModalContext = context;
			this.#observeIsSelected();
		});
	}

	#observeIsSelected() {
		const selectionManager = this.#pickerModalContext?.selection;
		if (!selectionManager) return;

		const unique = this.item?.unique;
		if (unique === undefined) return;

		this.observe(selectionManager.selection, () => {
			this._isSelected = selectionManager.isSelected(unique);
			console.log(this._isSelected);
		});
	}

	override render() {
		const item = this.item;
		if (!item) return nothing;

		return html`
			<uui-ref-node
				name=${item.name}
				id=${item.unique}
				readonly
				selectable
				@selected=${() => this.#pickerModalContext?.selection.select(item.unique)}
				@deselected=${() => this.#pickerModalContext?.selection.deselect(item.unique)}
				?selected=${this._isSelected}>
				${this.#renderIcon()}
			</uui-ref-node>
		`;
	}

	#renderIcon() {
		if (!this.item?.icon) return nothing;
		return html`<umb-icon slot="icon" name=${this.item.icon}></umb-icon>`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDefaultPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDefaultPickerSearchResultItemElement;
	}
}
