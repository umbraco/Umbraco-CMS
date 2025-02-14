import type { UmbPickerContext } from '../../picker.context.js';
import { UMB_PICKER_CONTEXT } from '../../picker.context.token.js';
import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
import { html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbPickerSearchResultItemElementBase<ItemType extends UmbNamedEntityModel> extends UmbLitElement {
	#item: ItemType | undefined;

	protected pickerContext?: UmbPickerContext;

	@property({ type: Object })
	public set item(value: ItemType | undefined) {
		this.#item = value;
		this.#observeIsSelected();
	}
	public get item(): ItemType | undefined {
		return this.#item;
	}

	@state()
	_isSelected = false;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_CONTEXT, (context) => {
			this.pickerContext = context;
			this.#observeIsSelected();
		});
	}

	#observeIsSelected() {
		const selectionManager = this.pickerContext?.selection;
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
				select-only
				selectable
				?selected=${this._isSelected}
				@deselected=${() => this.pickerContext?.selection.deselect(item.unique)}
				@selected=${() => this.pickerContext?.selection.select(item.unique)}>
			</umb-ref-item>
		`;
	}
}
