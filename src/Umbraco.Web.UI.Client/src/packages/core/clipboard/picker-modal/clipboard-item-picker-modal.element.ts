import { UmbClipboardCollectionRepository } from '../collection/index.js';
import type {
	UmbClipboardItemPickerModalValue,
	UmbClipboardItemPickerModalData,
} from './clipboard-item-picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbClipboardEntry } from '../types.js';

@customElement('umb-clipboard-item-picker-modal')
export class UmbClipboardItemPickerModalElement extends UmbModalBaseElement<
	UmbClipboardItemPickerModalData,
	UmbClipboardItemPickerModalValue
> {
	@state()
	private _items: Array<UmbClipboardEntry> = [];

	#collectionRepository = new UmbClipboardCollectionRepository(this);
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
		return html`<umb-body-layout headline="Clipboard">
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
					: html`There are no items in the clipboard`}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export { UmbClipboardItemPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-clipboard-item-picker-modal': UmbClipboardItemPickerModalElement;
	}
}
