import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS } from '../conditions/constants.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
} from '../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbVariantPropertyViewState, UmbVariantPropertyWriteState } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbStateManager } from '@umbraco-cms/backoffice/utils';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

export class UmbDocumentPropertyValueUserPermissionWorkspaceContext extends UmbControllerBase {
	#documentWorkspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;
	#blockWorkspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#documentWorkspaceContext = context;
			this.#observeDocumentProperties();
		});

		// TODO: investigate if block workspace can provide a property structure context so we can use the same context for both block and document
		// TODO: only apply to blocks within a document
		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, async (context) => {
			this.#blockWorkspaceContext = context;
			this.#observeDocumentBlockProperties();
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

				this.#setPermissions(
					properties,
					variantIds,
					structureManager.propertyViewState,
					structureManager.propertyWriteState,
				);
			},
		);
	}

	async #observeDocumentBlockProperties() {
		if (!this.#blockWorkspaceContext) return;
		const datasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
		if (!datasetContext) return;

		const structureManager = this.#blockWorkspaceContext.content.structure;

		this.observe(
			observeMultiple([structureManager.contentTypeProperties, this.#blockWorkspaceContext.variantId]),
			([properties, variantId]) => {
				if (properties.length === 0) return;
				if (!variantId) return;

				this.#setPermissions(
					properties,
					[variantId],
					structureManager.propertyViewState,
					structureManager.propertyWriteState,
				);
			},
		);
	}

	#setPermissions(
		properties: Array<UmbPropertyTypeModel>,
		variantIds: Array<UmbVariantId>,
		propertyVisibilityState: UmbStateManager<UmbVariantPropertyViewState>,
		propertyWriteState: UmbStateManager<UmbVariantPropertyWriteState>,
	) {
		properties.forEach((property) => {
			this.#setPermissionForProperty({
				verb: UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
				stateManager: propertyVisibilityState,
				property,
				variantIds,
			});

			this.#setPermissionForProperty({
				verb: UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
				stateManager: propertyWriteState,
				property,
				variantIds,
			});
		});
	}

	#setPermissionForProperty(args: {
		verb: string;
		stateManager: UmbStateManager<UmbVariantPropertyViewState | UmbVariantPropertyWriteState>;
		property: UmbPropertyTypeModel;
		variantIds: Array<UmbVariantId>;
	}) {
		createExtensionApiByAlias(this, UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					allOf: [args.verb],
					match: {
						propertyType: {
							unique: args.property.unique,
						},
					},
				},
				onChange: (permitted: boolean) => {
					// If the property is invariant we only need one state for the property
					const isInvariant = args.property.variesByCulture === false && args.property.variesBySegment === false;
					const variantIds = isInvariant ? [new UmbVariantId()] : args.variantIds;

					const states: Array<UmbVariantPropertyWriteState> =
						variantIds?.map((variantId) => {
							return {
								unique: 'UMB_PROPERTY_' + args.property.unique + '_' + variantId.toString(),
								message: '',
								propertyType: {
									unique: args.property.unique,
									variantId,
								},
							};
						}) || [];

					if (permitted) {
						args.stateManager.addStates(states);
					} else {
						args.stateManager.removeStates(states.map((state) => state.unique));
					}
				},
			},
		]);
	}
}

export { UmbDocumentPropertyValueUserPermissionWorkspaceContext as api };
