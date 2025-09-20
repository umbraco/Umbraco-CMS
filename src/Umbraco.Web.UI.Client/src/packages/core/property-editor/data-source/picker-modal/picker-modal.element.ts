import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UmbPropertyEditorDataSourceCollectionRepository } from '../collection/index.js';
import type {
	UmbPropertyEditorDataSourcePickerModalData,
	UmbPropertyEditorDataSourcePickerModalValue,
} from './picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-property-editor-data-source-picker-modal')
export class UmbPropertyEditorDataSourcePickerModalElement extends UmbModalBaseElement<
	UmbPropertyEditorDataSourcePickerModalData,
	UmbPropertyEditorDataSourcePickerModalValue
> {
	@state()
	private _items: Array<UmbPropertyEditorDataSourceItemModel> = [];

	#collectionRepository = new UmbPropertyEditorDataSourceCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);

		this.observe(this.#selectionManager.selection, (selection) => {
			this.value = { selection };
		});
	}

	override async firstUpdated() {
		const { data } = await this.#collectionRepository.requestCollection({});
		debugger;
		this._items = data?.items ?? [];
	}

	get #filteredItems() {
		if (this.data?.filter) {
			return this._items.filter(this.data.filter);
		} else {
			return this._items;
		}
	}

	#submit() {
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline="Select">
			<uui-box>
				${this.#filteredItems.length > 0
					? repeat(
							this.#filteredItems,
							(item) => item.unique,
							(item) => html`
								<uui-menu-item
									label=${item.name ?? ''}
									selectable
									@selected=${() => this.#selectionManager.select(item.unique)}
									@deselected=${() => this.#selectionManager.deselect(item.unique)}
									?selected=${this.value.selection.includes(item.unique)}>
									<uui-icon slot="icon" name="icon-globe"></uui-icon>
								</uui-menu-item>
							`,
						)
					: html`<umb-localize key="language_noFallbackLanguages">
							There are no other languages to choose from
						</umb-localize>`}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export { UmbPropertyEditorDataSourcePickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-data-source-picker-modal': UmbPropertyEditorDataSourcePickerModalElement;
	}
}
