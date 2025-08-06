import { UMB_MENU_CONTEXT } from '../menu/index.js';
import type { ManifestMenuItem, UmbMenuItemExpansionEntryModel } from '../../types.js';
import { UMB_MENU_ITEM_CONTEXT } from './menu-item.context.token.js';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultMenuItemContext extends UmbContextBase {
	public readonly expansion = new UmbEntityExpansionManager<UmbMenuItemExpansionEntryModel>(this);
	public manifest?: ManifestMenuItem;

	#menuContext?: typeof UMB_MENU_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_ITEM_CONTEXT);

		this.consumeContext(UMB_MENU_CONTEXT, (context) => {
			this.#menuContext = context;
			this.#observeMenuExpansion();
		});
	}

	#observeMenuExpansion() {
		this.observe(this.#menuContext?.expansion.expansion, (items) => {
			const itemsForMenuItem = items?.filter((item) => item.menuItemAlias === this.manifest?.alias) || [];
			this.expansion.setExpansion(itemsForMenuItem);
		});
	}
}
