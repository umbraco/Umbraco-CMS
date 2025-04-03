import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UmbPropertyValueUserPermissionWorkspaceContextBase } from './property-value-user-permission-workspace-context-base.js';

export class UmbDocumentBlockPropertyValueUserPermissionWorkspaceContext extends UmbPropertyValueUserPermissionWorkspaceContextBase {
	#blockWorkspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, async (context) => {
			this.#blockWorkspaceContext = context;

			// We only want to apply the permission logic if the block is in a document
			// TODO: revisit this when getContext supports passContextAliasMatches
			const documentWorkspaceContext = await this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, () => {})
				.passContextAliasMatches()
				.asPromise()
				.catch(() => undefined);

			if (documentWorkspaceContext) {
				this.#observeDocumentBlockProperties();
			} else {
				// TODO: Revisit if we really want to limit the rules to Documents. [NL]
				// Silently ignore if the block is not in a document
			}
		});
	}

	async #observeDocumentBlockProperties() {
		if (!this.#blockWorkspaceContext) return;

		// TODO: why get the dataset context here? [NL]
		//const datasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
		//if (!datasetContext) return;

		const structureManager = this.#blockWorkspaceContext.content.structure;

		this.observe(structureManager.contentTypeProperties, (properties) => {
			// TODO: If zero properties I guess we should then clear the state? [NL]
			if (properties.length === 0) return;

			this._setPermissions(properties, structureManager.propertyViewGuard, structureManager.propertyWriteGuard);
		});
	}
}

export { UmbDocumentBlockPropertyValueUserPermissionWorkspaceContext as api };
