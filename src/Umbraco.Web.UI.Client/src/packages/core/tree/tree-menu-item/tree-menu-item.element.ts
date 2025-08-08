import type { ManifestMenuItemTreeKind } from './types.js';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MENU_ITEM_CONTEXT, type UmbMenuItemElement } from '@umbraco-cms/backoffice/menu';
import {
	UmbExpansionEntryCollapsedEvent,
	UmbExpansionEntryExpandedEvent,
	type UmbEntityExpansionModel,
} from '@umbraco-cms/backoffice/utils';

@customElement('umb-menu-item-tree-default')
export class UmbMenuItemTreeDefaultElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	@state()
	private _menuItemExpansion: UmbEntityExpansionModel = [];

	#menuItemContext?: typeof UMB_MENU_ITEM_CONTEXT.TYPE;
	#muteStateUpdate = false;

	constructor() {
		super();

		this.consumeContext(UMB_MENU_ITEM_CONTEXT, (context) => {
			this.#menuItemContext = context;
			this.#observeExpansion();
		});
	}

	#observeExpansion() {
		this.observe(this.#menuItemContext?.expansion.expansion, (items) => {
			if (this.#muteStateUpdate) return;
			this._menuItemExpansion = items || [];
		});
	}

	#onExpansionChange(event: UmbExpansionEntryExpandedEvent | UmbExpansionEntryCollapsedEvent) {
		event.stopPropagation();
		const eventEntry = event.entry;

		if (!eventEntry) {
			throw new Error('Entry is required to toggle expansion.');
		}

		if (!this.manifest) {
			throw new Error('Manifest is required to toggle expansion.');
		}

		if (event.type === UmbExpansionEntryExpandedEvent.TYPE) {
			this.#muteStateUpdate = true;
			this.#menuItemContext?.expansion.expandItem({ ...eventEntry, menuItemAlias: this.manifest.alias });
			this.#muteStateUpdate = false;
		} else if (event.type === UmbExpansionEntryCollapsedEvent.TYPE) {
			this.#muteStateUpdate = true;
			this.#menuItemContext?.expansion.collapseItem({ ...eventEntry, menuItemAlias: this.manifest.alias });
			this.#muteStateUpdate = false;
		}
	}

	override render() {
		return this.manifest
			? html`
					<umb-tree
						alias=${this.manifest?.meta.treeAlias}
						.props=${{
							hideTreeRoot: this.manifest?.meta.hideTreeRoot === true,
							selectionConfiguration: {
								selectable: false,
								multiple: false,
							},
							expansion: this._menuItemExpansion,
						}}
						@expansion-entry-expanded=${this.#onExpansionChange}
						@expansion-entry-collapsed=${this.#onExpansionChange}></umb-tree>
				`
			: nothing;
	}
}

export default UmbMenuItemTreeDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-tree-default': UmbMenuItemTreeDefaultElement;
	}
}
