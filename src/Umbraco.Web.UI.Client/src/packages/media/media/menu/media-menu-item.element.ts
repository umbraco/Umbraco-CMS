import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TREE_REPOSITORY_ALIAS } from '../tree/constants.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbMenuItemTreeDefaultElement,
	UmbTreeMenuItemStartNodeAutoExpansionController,
} from '@umbraco-cms/backoffice/tree';

@customElement('umb-media-menu-item')
export class UmbMediaMenuItemElement extends UmbMenuItemTreeDefaultElement {
	#startNodeAutoExpansion = new UmbTreeMenuItemStartNodeAutoExpansionController(this);

	constructor() {
		super();
		this.#startNodeAutoExpansion.setTreeRepositoryAlias(UMB_MEDIA_TREE_REPOSITORY_ALIAS);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				observeMultiple([context.hasMediaRootAccess, context.mediaStartNodeUniques]),
				([hasRootAccess, startNodeUniques]) => {
					this.#startNodeAutoExpansion.setStartNodes(
						hasRootAccess ? [] : startNodeUniques?.map(({ unique }) => ({ unique, entityType: UMB_MEDIA_ENTITY_TYPE })),
					);
				},
				'umbCurrentUserMediaStartNodesObserver',
			);
		});
	}
}

export { UmbMediaMenuItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-menu-item': UmbMediaMenuItemElement;
	}
}
