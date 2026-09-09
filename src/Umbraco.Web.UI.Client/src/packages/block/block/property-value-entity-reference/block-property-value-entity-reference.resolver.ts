import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/element';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueData,
	UmbPropertyValueEntityReferenceResolver,
} from '@umbraco-cms/backoffice/property';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// A Block Grid layout entry can nest further blocks inside named areas — List and Single layout entries never
// carry this field, so the walk below is a no-op for them.
interface UmbBlockLayoutEntryWithAreas extends UmbBlockLayoutBaseModel {
	areas?: Array<{ items: Array<UmbBlockLayoutBaseModel> }>;
}

/**
 * Resolves the elements a Block List/Grid/Single value references as external content (i.e. reused rather than
 * owned by the block). Shared across all three block editors — each registers its own manifest with a matching
 * `forEditorAlias`, and this resolver reads that alias back off {@link manifest} to find its layout entries.
 */
export class UmbBlockPropertyValueEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
	manifest?: ManifestPropertyValueEntityReference;

	async resolveEntityReferences(
		value: UmbPropertyValueData<UmbBlockValueType>,
	): Promise<Array<UmbEntityModel>> {
		const schemaAlias = this.manifest?.forEditorAlias;
		if (!schemaAlias) return [];

		const layout = value.value?.layout?.[schemaAlias] as Array<UmbBlockLayoutEntryWithAreas> | undefined;
		if (!layout) return [];

		return this.#collectExternalContentReferences(layout);
	}

	#collectExternalContentReferences(entries: Array<UmbBlockLayoutEntryWithAreas>): Array<UmbEntityModel> {
		const references: Array<UmbEntityModel> = [];
		for (const entry of entries) {
			if (entry.isExternalContent && entry.contentKey) {
				references.push({ entityType: UMB_ELEMENT_ENTITY_TYPE, unique: entry.contentKey });
			}
			for (const area of entry.areas ?? []) {
				references.push(...this.#collectExternalContentReferences(area.items));
			}
		}
		return references;
	}

	destroy(): void {}
}
