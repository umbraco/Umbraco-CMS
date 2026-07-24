import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TREE_REPOSITORY_ALIAS } from '../tree/constants.js';
import type { UmbMediaTreeItemModel, UmbMediaTreeRootModel } from '../tree/types.js';
import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/tree';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MENU_ITEM_CONTEXT, type UmbMenuItemElement } from '@umbraco-cms/backoffice/menu';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import {
	UmbExpansionEntryCollapsedEvent,
	UmbExpansionEntryExpandedEvent,
	linkEntityExpansionEntries,
	type UmbEntityExpansionModel,
} from '@umbraco-cms/backoffice/utils';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

@customElement('umb-media-menu-item')
export class UmbMediaMenuItemElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	@state()
	private _menuItemExpansion: UmbEntityExpansionModel = [];

	#menuItemContext?: typeof UMB_MENU_ITEM_CONTEXT.TYPE;
	#muteStateUpdate = false;
	#hasAutoExpanded = false;

	constructor() {
		super();

		this.consumeContext(UMB_MENU_ITEM_CONTEXT, (context) => {
			this.#menuItemContext = context;
			this.#observeExpansion();
			this.#autoExpandToStartNodes();
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

	async #autoExpandToStartNodes() {
		if (this.#hasAutoExpanded) return;
		if (!this.manifest) return;

		let currentUserContext: typeof UMB_CURRENT_USER_CONTEXT.TYPE | undefined;
		try {
			currentUserContext = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		} catch {
			return;
		}
		if (!currentUserContext) return;

		const currentUser = await firstValueFrom(currentUserContext.currentUser);
		if (!currentUser) return;

		const startNodeUniques = currentUser.mediaStartNodeUniques;
		if (!startNodeUniques || startNodeUniques.length === 0) return;
		if (currentUser.hasMediaRootAccess) return;

		this.#hasAutoExpanded = true;

		const repo = await createExtensionApiByAlias<UmbTreeRepository<UmbMediaTreeItemModel, UmbMediaTreeRootModel>>(
			this,
			UMB_MEDIA_TREE_REPOSITORY_ALIAS,
		);
		if (!repo) return;

		for (const startNode of startNodeUniques) {
			try {
				const { data: ancestors } = await repo.requestTreeItemAncestors({
					treeItem: { unique: startNode.unique, entityType: UMB_MEDIA_ENTITY_TYPE },
				});

				if (!ancestors || ancestors.length === 0) continue;

				const linkedEntries = linkEntityExpansionEntries(ancestors);
				const entriesWithMenuAlias = linkedEntries.map((entry) => ({
					...entry,
					menuItemAlias: this.manifest!.alias,
				}));

				this.#menuItemContext?.expansion.expandItems(entriesWithMenuAlias);
			} catch {
				// Silently skip if ancestor fetch fails
			}
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
							isMenu: true,
						}}
						@expansion-entry-expanded=${this.#onExpansionChange}
						@expansion-entry-collapsed=${this.#onExpansionChange}></umb-tree>
				`
			: nothing;
	}
}

export { UmbMediaMenuItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-menu-item': UmbMediaMenuItemElement;
	}
}
