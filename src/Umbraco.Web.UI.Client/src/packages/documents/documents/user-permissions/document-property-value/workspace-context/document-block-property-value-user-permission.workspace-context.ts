import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UmbPropertyValueUserPermissionWorkspaceContextBase } from './property-value-user-permission-workspace-context-base.js';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';

export class UmbDocumentBlockPropertyValueUserPermissionWorkspaceContext extends UmbPropertyValueUserPermissionWorkspaceContextBase {
	#blockWorkspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, async (context) => {
			this.#blockWorkspaceContext = context;

			// We only want to apply the permission logic if the block is in a document
			// TODO: revisit this when getContext supports passContextAliasMatches
			const contentWorkspaceContext = await this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, () => {})
				.passContextAliasMatches()
				.asPromise()
				.catch(() => undefined);

			if (contentWorkspaceContext?.getEntityType() === UMB_DOCUMENT_ENTITY_TYPE) {
				this.#blockWorkspaceContext.content.propertyViewGuard.fallbackToDisallowed();
				this.#blockWorkspaceContext.content.propertyWriteGuard.fallbackToDisallowed();
				this.#observeDocumentBlockProperties();
			}
		});
	}

	async #observeDocumentBlockProperties() {
		if (!this.#blockWorkspaceContext) return;

		const owner = this.#blockWorkspaceContext.content;

		this.observe(owner.structure.contentTypeProperties, (properties) => {
			// TODO: If zero properties I guess we should then clear the state? [NL]
			if (properties.length === 0) return;

			this._setPermissions(properties, owner.propertyViewGuard, owner.propertyWriteGuard);
		});

		const ownerSettings = this.#blockWorkspaceContext.settings;

		this.observe(ownerSettings.structure.contentTypeProperties, (properties) => {
			// TODO: If zero properties I guess we should then clear the state? [NL]
			if (properties.length === 0) return;

			this._setPermissions(properties, ownerSettings.propertyViewGuard, ownerSettings.propertyWriteGuard);
		});
	}
}

export { UmbDocumentBlockPropertyValueUserPermissionWorkspaceContext as api };
