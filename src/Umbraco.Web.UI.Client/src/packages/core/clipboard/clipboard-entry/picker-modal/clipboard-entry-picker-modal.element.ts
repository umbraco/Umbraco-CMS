import { UmbClipboardCollectionRepository } from '../../collection/index.js';
import type { UmbClipboardEntryDetailModel } from '../types.js';
import type {
	UmbClipboardEntryPickerModalValue,
	UmbClipboardEntryPickerModalData,
} from './clipboard-entry-picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

@customElement('umb-clipboard-entry-picker-modal')
export class UmbClipboardEntryPickerModalElement extends UmbModalBaseElement<
	UmbClipboardEntryPickerModalData,
	UmbClipboardEntryPickerModalValue
> {
	@state()
	private _items: Array<UmbClipboardEntryDetailModel> = [];

	#collectionRepository = new UmbClipboardCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);
	#entityContext = new UmbEntityContext(this);
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor() {
		super();
		this.#entityContext.setEntityType('clipboard-entry');
		this.#entityContext.setUnique(null);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);

		this.observe(this.#selectionManager.selection, (selection) => {
			this.value = { selection };
		});

		this.#listenToEntityEvents();
	}

	override async firstUpdated() {
		this.#requestItems();
	}

	async #requestItems() {
		const entryType = this.data?.entry?.type;

		const { data } = await this.#collectionRepository.requestCollection({ entry: { type: entryType } });
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

	async #listenToEntityEvents() {
		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;

			context?.removeEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureRequest as unknown as EventListener,
			);

			context?.removeEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadChildrenRequest as unknown as EventListener,
			);

			context?.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureRequest as unknown as EventListener,
			);

			context?.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadChildrenRequest as unknown as EventListener,
			);
		});
	}

	#onReloadStructureRequest = (event: UmbRequestReloadStructureForEntityEvent) => {
		const hasItem = this._items.some((item) => item.unique === event.getUnique());
		if (hasItem) {
			this.#requestItems();
		}
	};

	#onReloadChildrenRequest = async (event: UmbRequestReloadChildrenOfEntityEvent) => {
		// check if the collection is in the same context as the entity from the event
		const unique = this.#entityContext.getUnique();
		const entityType = this.#entityContext.getEntityType();

		if (unique === event.getUnique() && entityType === event.getEntityType()) {
			this.#requestItems();
		}
	};

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
									<uui-icon slot="icon" name="icon-clipboard-entry"></uui-icon>
									<umb-entity-actions-bundle
										slot="actions"
										.entityType=${item.entityType}
										.unique=${item.unique}
										.label=${item.name}>
									</umb-entity-actions-bundle>
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

	override destroy(): void {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureRequest as unknown as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadChildrenRequest as unknown as EventListener,
		);

		super.destroy();
	}
}

export { UmbClipboardEntryPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-clipboard-entry-picker-modal': UmbClipboardEntryPickerModalElement;
	}
}
