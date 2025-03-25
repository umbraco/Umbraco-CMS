import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
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

		this.observe(
			observeMultiple([
				this.#documentWorkspaceContext.structure.contentTypeProperties,
				this.#documentWorkspaceContext.variantOptions,
			]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				const variantIds = variantOptions?.map((variant) => new UmbVariantId(variant.culture, variant.segment));

				this._setPermissions(
					properties,
					variantIds,
					structureManager.propertyViewState,
					structureManager.propertyWriteState,
				);
			},
		);
	}
}

export { UmbDocumentPropertyValueUserPermissionWorkspaceContext as api };
