import type { ManifestMenuItemTreeKind } from './types.js';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MENU_CONTEXT, type UmbMenuItemElement } from '@umbraco-cms/backoffice/menu';
import {
	UmbExpansionEntityCollapsedEvent,
	UmbExpansionEntityExpandedEvent,
	type UmbEntityExpansionModel,
} from '@umbraco-cms/backoffice/utils';

@customElement('umb-menu-item-tree-default')
export class UmbMenuItemTreeDefaultElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	@state()
	_menuExpansion: UmbEntityExpansionModel = [];

	#menuContext?: typeof UMB_MENU_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MENU_CONTEXT, (context) => {
			this.#menuContext = context;
			this.#observeExpansion();
		});
	}

	#observeExpansion() {
		this.observe(this.#menuContext?.expansion.expansion, (items) => {
			this._menuExpansion = items || [];
		});
	}

	#onEntityExpansionChange(event: UmbExpansionEntityExpandedEvent | UmbExpansionEntityCollapsedEvent) {
		event.stopPropagation();
		const eventEntity = event.entity;

		if (!eventEntity) {
			throw new Error('Entity is required to toggle expansion.');
		}

		if (event.type === UmbExpansionEntityExpandedEvent.TYPE) {
			this.#menuContext?.expansion.expandItem(eventEntity);
		} else if (event.type === UmbExpansionEntityCollapsedEvent.TYPE) {
			this.#menuContext?.expansion.collapseItem(eventEntity);
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
							expansion: this._menuExpansion,
						}}
						@expansion-entity-expanded=${this.#onEntityExpansionChange}
						@expansion-entity-collapsed=${this.#onEntityExpansionChange}></umb-tree>
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
