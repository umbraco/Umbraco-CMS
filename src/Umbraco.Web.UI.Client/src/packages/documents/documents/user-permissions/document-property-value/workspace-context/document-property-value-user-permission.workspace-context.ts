import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../workspace/constants.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS } from '../conditions/constants.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
} from '../constants.js';
import type { UmbDocumentVariantModel } from '../../../types.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_WORKSPACE_CONTEXT } from './document-property-value-user-permission.workspace-context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbVariantId,
	type UmbEntityVariantOptionModel,
	type UmbVariantPropertyVisibilityState,
	type UmbVariantPropertyWriteState,
} from '@umbraco-cms/backoffice/variant';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbStateManager } from '@umbraco-cms/backoffice/utils';

export class UmbDocumentPropertyValueUserPermissionWorkspaceContext extends UmbContextBase<UmbDocumentPropertyValueUserPermissionWorkspaceContext> {
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeStructure();
		});
	}

	#observeStructure() {
		if (!this.#workspaceContext) return;

		this.observe(
			observeMultiple([this.#workspaceContext.structure.contentTypeProperties, this.#workspaceContext.variantOptions]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				properties.forEach((property) => {
					this.#setPermissionForProperty({
						verb: UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
						stateManager: this.#workspaceContext!.structure.propertyVisibilityState,
						property,
						variantOptions,
					});

					this.#setPermissionForProperty({
						verb: UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
						stateManager: this.#workspaceContext!.structure.propertyWriteState,
						property,
						variantOptions,
					});
				});
			},
		);
	}

	#setPermissionForProperty(args: {
		verb: string;
		stateManager: UmbStateManager<UmbVariantPropertyVisibilityState | UmbVariantPropertyWriteState>;
		property: UmbPropertyTypeModel;
		variantOptions: Array<UmbEntityVariantOptionModel<UmbDocumentVariantModel>>;
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
					const variantIds = isInvariant
						? [new UmbVariantId()]
						: args.variantOptions?.map((variant) => new UmbVariantId(variant.culture, variant.segment)) || [];

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
