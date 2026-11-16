import type { UmbPropertyEditorRteValueType } from '../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/element';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueData,
	UmbPropertyValueEntityReferenceResolver,
} from '@umbraco-cms/backoffice/property';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Resolves the elements a rich text value references as external content (i.e. reused rather than owned by the
 * inline block).
 */
export class UmbRtePropertyValueEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
	manifest?: ManifestPropertyValueEntityReference;

	async resolveEntityReferences(
		value: UmbPropertyValueData<UmbPropertyEditorRteValueType>,
	): Promise<Array<UmbEntityModel>> {
		const schemaAlias = this.manifest?.forEditorAlias;
		if (!schemaAlias) return [];

		const layout = value.value?.blocks?.layout?.[schemaAlias];
		if (!layout) return [];

		return layout
			.filter((entry) => entry.isExternalContent && entry.contentKey)
			.map((entry) => ({ entityType: UMB_ELEMENT_ENTITY_TYPE, unique: entry.contentKey as string }));
	}

	destroy(): void {}
}
