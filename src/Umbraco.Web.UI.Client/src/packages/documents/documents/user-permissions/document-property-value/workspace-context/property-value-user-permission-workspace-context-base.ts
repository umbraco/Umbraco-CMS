import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS } from '../conditions/constants.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
} from '../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbVariantPropertyViewState, UmbVariantPropertyWriteState } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbStateManager } from '@umbraco-cms/backoffice/utils';

export class UmbPropertyValueUserPermissionWorkspaceContextBase extends UmbControllerBase {
	protected _setPermissions(
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
		// TODO: Oh, this results in quite a few Context Consumptions. Lets try not to use a condition in this case. [NL]
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
								state: true,
								unique: 'UMB_PROPERTY_' + args.property.unique + '_' + variantId.toString(),
								propertyType: {
									unique: args.property.unique,
								},
								variantId,
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

export { UmbPropertyValueUserPermissionWorkspaceContextBase as api };
