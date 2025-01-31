import { UmbClipboardCollectionRepository } from '../../collection/index.js';
import type { UmbClipboardEntryDetailModel } from '../types.js';
import { html, customElement, state, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbEntityContext, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// TODO: make this into an extension point (Picker) with two kinds of pickers: tree-item-picker and collection-item-picker;
@customElement('umb-clipboard-entry-picker')
export class UmbClipboardEntryPickerElement extends UmbLitElement {
	@property({ type: Array })
	selection: Array<UmbEntityUnique> = [];

	@property({ type: Object })
	config?: any;

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
		this.#selectionManager.setMultiple(this.config?.multiple ?? false);
		this.#selectionManager.setSelection(this.selection ?? []);

		this.observe(this.#selectionManager.selection, (selection) => {
			this.selection = selection;
		});

		this.#listenToEntityEvents();
	}

	override async firstUpdated() {
		this.#requestItems();
	}

	async #requestItems() {
		const { data } = await this.#collectionRepository.requestCollection({
			types: this.config?.entryTypes ?? [],
		});

		const entries = data?.items ?? [];
		const sortedEntries = entries.sort((a, b) => new Date(b.updateDate!).getTime() - new Date(a.updateDate!).getTime());

		if (this.config?.filter) {
			this._items = sortedEntries.filter(this.config.filter);
			return;
		}

		if (this.config?.asyncFilter) {
			const promises = Promise.all(sortedEntries.map(this.config.asyncFilter));
			const results = await promises;
			this._items = sortedEntries.filter((_, index) => results[index]);
			return;
		}

		this._items = sortedEntries;
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
		return html`${this._items.length > 0
			? repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)
			: html`There are no items in the clipboard`}`;
	}

	#renderItem(item: UmbClipboardEntryDetailModel) {
		return html`
			<uui-menu-item
				label=${item.name ?? ''}
				selectable
				@selected=${() => this.#selectionManager.select(item.unique)}
				@deselected=${() => this.#selectionManager.deselect(item.unique)}
				?selected=${this.selection.includes(item.unique)}>
				${this.#renderItemIcon(item)} ${this.#renderItemActions(item)}
			</uui-menu-item>
		`;
	}

	#renderItemIcon(item: UmbClipboardEntryDetailModel) {
		const iconName = item.icon ?? 'icon-clipboard-entry';
		return html`<uui-icon slot="icon" name=${iconName}></uui-icon>`;
	}

	#renderItemActions(item: UmbClipboardEntryDetailModel) {
		return html`
			<umb-entity-actions-bundle
				slot="actions"
				.entityType=${item.entityType}
				.unique=${item.unique}
				.label=${item.name}>
			</umb-entity-actions-bundle>
		`;
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-clipboard-entry-picker': UmbClipboardEntryPickerElement;
	}
}
