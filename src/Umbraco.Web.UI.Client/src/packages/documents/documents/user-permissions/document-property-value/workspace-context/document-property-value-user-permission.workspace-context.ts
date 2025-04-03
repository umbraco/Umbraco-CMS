import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyValueUserPermissionWorkspaceContextBase } from './property-value-user-permission-workspace-context-base.js';

export class UmbDocumentPropertyValueUserPermissionWorkspaceContext extends UmbPropertyValueUserPermissionWorkspaceContextBase {
	#documentWorkspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#documentWorkspaceContext = context;
			this.#observeDocumentProperties();
		});
	}

	#observeDocumentProperties() {
		if (!this.#documentWorkspaceContext) return;

		const structureManager = this.#documentWorkspaceContext.structure;

		this.observe(this.#documentWorkspaceContext.structure.contentTypeProperties, (properties) => {
			// TODO: If zero properties I guess we should then clear the state? [NL]
			if (properties.length === 0) return;

			this._setPermissions(properties, structureManager.propertyViewGuard, structureManager.propertyWriteGuard);
		});
	}
}

export { UmbDocumentPropertyValueUserPermissionWorkspaceContext as api };
