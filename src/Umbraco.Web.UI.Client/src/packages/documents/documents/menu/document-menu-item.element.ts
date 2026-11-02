import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbMenuItemTreeDefaultElement,
	UmbTreeMenuItemStartNodeAutoExpansionController,
} from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-menu-item')
export class UmbDocumentMenuItemElement extends UmbMenuItemTreeDefaultElement {
	#startNodeAutoExpansion = new UmbTreeMenuItemStartNodeAutoExpansionController(this);

	constructor() {
		super();
		this.#startNodeAutoExpansion.setTreeRepositoryAlias(UMB_DOCUMENT_TREE_REPOSITORY_ALIAS);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				observeMultiple([context.hasDocumentRootAccess, context.documentStartNodeUniques]),
				([hasRootAccess, startNodeUniques]) => {
					this.#startNodeAutoExpansion.setStartNodes(
						hasRootAccess
							? []
							: startNodeUniques?.map(({ unique }) => ({ unique, entityType: UMB_DOCUMENT_ENTITY_TYPE })),
					);
				},
				'umbCurrentUserDocumentStartNodesObserver',
			);
		});
	}
}

export { UmbDocumentMenuItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-menu-item': UmbDocumentMenuItemElement;
	}
}
