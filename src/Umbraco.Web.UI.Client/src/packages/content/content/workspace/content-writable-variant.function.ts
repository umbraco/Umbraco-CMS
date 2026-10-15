import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import type { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import type { UmbVariantId, UmbObjectWithVariantProperties } from '@umbraco-cms/backoffice/variant';
import type { UmbVariantNameWriteGuardManager } from '@umbraco-cms/backoffice/workspace';

/**
 * @internal
 */
export interface UmbContentWritableVariantArgs {
	variantId: UmbVariantId;
	variantOptions: ReadonlyArray<UmbObjectWithVariantProperties>;
	properties: ReadonlyArray<UmbPropertyTypeModel>;
	readOnlyGuard: UmbReadOnlyVariantGuardManager;
	nameWriteGuard: UmbVariantNameWriteGuardManager;
	propertyWriteGuard: UmbVariantPropertyGuardManager;
}

/**
 * Resolves if a variant is writable: it is not read-only, and its own data has something that may be written — its
 * name or any property that belongs to it. A property belongs to the variant that matches how the property varies, so
 * data that is shared between variants belongs to the invariant variant.
 * @internal Not part of the public API; use `isWritableVariant` / `getIsWritableVariant` on the content workspace context.
 * @param {UmbContentWritableVariantArgs} args - The variant and the guards and structure to resolve it from
 * @returns {boolean} true if the variant is writable
 */
export function _resolveIsWritableVariant(args: UmbContentWritableVariantArgs): boolean {
	const { variantId, variantOptions, properties, readOnlyGuard, nameWriteGuard, propertyWriteGuard } = args;

	if (readOnlyGuard.getIsPermittedForVariant(variantId)) {
		return false;
	}

	if (variantOptions.some((option) => variantId.compare(option)) && nameWriteGuard.getIsPermittedForName(variantId)) {
		return true;
	}

	return properties.some((property) => {
		const belongsToVariant =
			Boolean(property.variesByCulture) === (variantId.culture !== null) &&
			Boolean(property.variesBySegment) === (variantId.segment !== null);
		return belongsToVariant && propertyWriteGuard.getIsPermittedForVariantAndProperty(variantId, property, variantId);
	});
}
