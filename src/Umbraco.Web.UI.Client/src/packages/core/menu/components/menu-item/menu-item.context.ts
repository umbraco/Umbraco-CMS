import type { ManifestMenuItem } from '../../types.js';
import { UmbMenuItemExpansionManager } from './expansion/menu-item-expansion.manager.js';
import { UMB_MENU_ITEM_CONTEXT } from './menu-item.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultMenuItemContext extends UmbContextBase {
	public readonly expansion = new UmbMenuItemExpansionManager(this);

	#manifest?: ManifestMenuItem | undefined;
	public get manifest(): ManifestMenuItem | undefined {
		return this.#manifest;
	}
	public set manifest(value: ManifestMenuItem | undefined) {
		this.#manifest = value;
		this.expansion.setMenuItemAlias(value?.alias);
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_ITEM_CONTEXT);
	}
}
