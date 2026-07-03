import type { UmbMediaTreeItemModel, UmbMediaTreeRootItemsRequestArgs, UmbMediaTreeRootModel } from './types.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TREE_REPOSITORY_ALIAS } from './constants.js';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbDefaultTreeContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from '@umbraco-cms/backoffice/menu';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export class UmbMediaTreeContext extends UmbDefaultTreeContext<
	UmbMediaTreeItemModel,
	UmbMediaTreeRootModel,
	UmbMediaTreeRootItemsRequestArgs
> {
	#hasAutoExpanded = false;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.dataType, (value) => {
				this.updateAdditionalRequestArgs({ dataType: value });
			});
		});

		this.#autoExpandToStartNodes();
	}

	async #autoExpandToStartNodes() {
		if (this.#hasAutoExpanded) return;

		let sidebarMenuContext: typeof UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT.TYPE | undefined;
		try {
			sidebarMenuContext = await this.getContext(UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT);
		} catch {
			return;
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
					menuItemAlias: 'Umb.MenuItem.Media',
				}));

				sidebarMenuContext.expansion.expandItems(entriesWithMenuAlias);
			} catch {
				// Silently skip if ancestor fetch fails
			}
		}
	}
}

export { UmbMediaTreeContext as api };
