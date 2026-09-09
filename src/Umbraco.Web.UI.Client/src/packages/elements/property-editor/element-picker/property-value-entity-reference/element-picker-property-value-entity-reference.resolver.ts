import { UMB_ELEMENT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyValueEntityReferenceResolver } from '@umbraco-cms/backoffice/property';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/** Resolves the elements picked by an Element Picker value (a flat array of element uniques). */
export class UmbElementPickerPropertyValueEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
	async resolveEntityReferences(
		value: UmbPropertyValueData<Array<string> | undefined>,
	): Promise<Array<UmbEntityModel>> {
		return (value.value ?? []).map((unique) => ({ entityType: UMB_ELEMENT_ENTITY_TYPE, unique }));
	}

	destroy(): void {}
}
