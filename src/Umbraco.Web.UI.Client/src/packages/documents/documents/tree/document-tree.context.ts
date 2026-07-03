import type {
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
	UmbDocumentTreeRootModel,
} from './types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from './manifests.js';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbDefaultTreeContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from '@umbraco-cms/backoffice/menu';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentTreeContext extends UmbDefaultTreeContext<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel,
	UmbDocumentTreeRootItemsRequestArgs
> {
	#hasAutoExpanded = false;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context?.dataType,
				(value) => {
					if (value === undefined) return;
					this.updateAdditionalRequestArgs({ dataType: value });
				},
				'umbDocumentTreeDataTypeObserver',
			);
		});

		this.#autoExpandToStartNodes();
	}

	async #autoExpandToStartNodes() {
		if (this.#hasAutoExpanded) return;

		// Only auto-expand when used in the section sidebar menu (not in pickers)
		let sidebarMenuContext: typeof UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT.TYPE | undefined;
		try {
			sidebarMenuContext = await this.getContext(UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT);
		} catch {
			return; // Not in sidebar menu context — skip
		}
		if (!sidebarMenuContext) return;

		let currentUserContext: typeof UMB_CURRENT_USER_CONTEXT.TYPE | undefined;
		try {
			currentUserContext = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		} catch {
			return;
		}
		if (!currentUserContext) return;

		const currentUser = await firstValueFrom(currentUserContext.currentUser);
		if (!currentUser) return;

		const startNodeUniques = currentUser.documentStartNodeUniques;
		if (!startNodeUniques || startNodeUniques.length === 0) return;

		// Don't auto-expand if user has root access (they see everything already)
		if (currentUser.hasDocumentRootAccess) return;

		this.#hasAutoExpanded = true;

		// Get a tree repository to fetch ancestors
		const repo = await createExtensionApiByAlias<UmbTreeRepository<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>>(
			this,
			UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
		);
		if (!repo) return;

		for (const startNode of startNodeUniques) {
			try {
				const { data: ancestors } = await repo.requestTreeItemAncestors({
					treeItem: { unique: startNode.unique, entityType: UMB_DOCUMENT_ENTITY_TYPE },
				});

				if (!ancestors || ancestors.length === 0) continue;

				const linkedEntries = linkEntityExpansionEntries(ancestors);
				const entriesWithMenuAlias = linkedEntries.map((entry) => ({
					...entry,
					menuItemAlias: 'Umb.MenuItem.Document',
				}));

				sidebarMenuContext.expansion.expandItems(entriesWithMenuAlias);
			} catch {
				// Silently skip if ancestor fetch fails for a start node
			}
		}
	}
}

export { UmbDocumentTreeContext as api };
