import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import { UmbPropertyValueUserPermissionWorkspaceContextBase } from './property-value-user-permission-workspace-context-base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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

		const owner = this.#documentWorkspaceContext;

		this.observe(this.#documentWorkspaceContext.structure.contentTypeProperties, (properties) => {
			// TODO: If zero properties I guess we should then clear the state? [NL]
			if (!properties || properties.length === 0) return;

			this.#documentWorkspaceContext!.propertyViewGuard.fallbackToNotPermitted();
			this.#documentWorkspaceContext!.propertyWriteGuard.fallbackToNotPermitted();
			this._setPermissions(properties, owner.propertyViewGuard, owner.propertyWriteGuard);
		});
	}
}

export { UmbDocumentPropertyValueUserPermissionWorkspaceContext as api };
