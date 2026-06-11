import type {
	UmbVisualEditorPropertyGroup,
	UmbVisualEditorPropertyInfo,
} from './visual-editor-property-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { DataTypeService, DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export interface UmbVisualEditorBlockStructure {
	name: string;
	properties: UmbVisualEditorPropertyInfo[];
	groups: UmbVisualEditorPropertyGroup[];
}

/**
 * Resolves and caches property structures for the visual editor:
 * - Document properties across the full composition chain, indexed by alias.
 * - Block content/settings element type structures, cached by content type key.
 */
export class UmbVisualEditorPropertyStructureResolver extends UmbControllerBase {
	#documentProperties = new Map<string, UmbVisualEditorPropertyInfo>();
	#blockStructureCache = new Map<string, UmbVisualEditorBlockStructure>();

	get documentProperties(): Array<UmbVisualEditorPropertyInfo> {
		return [...this.#documentProperties.values()];
	}

	getDocumentProperty(alias: string): UmbVisualEditorPropertyInfo | undefined {
		return this.#documentProperties.get(alias);
	}

	/** Fetch the document type (and its composition chain) and index its properties by alias. */
	async loadDocumentStructure(contentTypeUnique: string): Promise<void> {
		const fetchedIds = new Set<string>();
		const toFetch = [contentTypeUnique];
		const allProperties: Array<{
			alias: string;
			name: string;
			description?: string | null;
			dataType: { id: string };
			validation: any;
		}> = [];

		while (toFetch.length > 0) {
			const batch = [...toFetch];
			toFetch.length = 0;

			for (const id of batch) {
				if (fetchedIds.has(id)) continue;
				fetchedIds.add(id);

				const { data } = await tryExecute(this, DocumentTypeService.getDocumentTypeById({ path: { id } }));
				if (!data) continue;

				allProperties.push(...(data.properties ?? []));

				for (const comp of data.compositions ?? []) {
					if (!fetchedIds.has(comp.documentType.id)) {
						toFetch.push(comp.documentType.id);
					}
				}
			}
		}

		this.#documentProperties.clear();
		for (const prop of allProperties) {
			const { editorUiAlias, config } = await this.#resolveDataType(prop.dataType.id);

			this.#documentProperties.set(prop.alias, {
				alias: prop.alias,
				name: prop.name ?? prop.alias,
				description: prop.description ?? undefined,
				editorUiAlias,
				config,
				validation: prop.validation,
			});
		}
	}

	/** Resolve a block element type's properties and groups, cached by content type key. */
	async resolveBlockPropertyStructures(contentTypeKey: string): Promise<UmbVisualEditorBlockStructure> {
		const cached = this.#blockStructureCache.get(contentTypeKey);
		if (cached) return cached;

		const { data } = await tryExecute(this, DocumentTypeService.getDocumentTypeById({ path: { id: contentTypeKey } }));
		if (!data?.properties) return { name: 'Block', properties: [], groups: [] };

		const groups: UmbVisualEditorPropertyGroup[] = (data.containers ?? [])
			.filter((c) => c.type === 'Group')
			.map((c) => ({ id: c.id, name: c.name ?? '', sortOrder: c.sortOrder }))
			.sort((a, b) => a.sortOrder - b.sortOrder);

		const result: UmbVisualEditorPropertyInfo[] = [];
		for (const prop of data.properties) {
			const { editorUiAlias, config } = await this.#resolveDataType(prop.dataType.id);

			result.push({
				alias: prop.alias,
				name: prop.name ?? prop.alias,
				description: prop.description ?? undefined,
				editorUiAlias: editorUiAlias || 'Umb.PropertyEditorUi.TextBox',
				config,
				validation: prop.validation,
				containerId: prop.container?.id ?? null,
			});
		}

		const resolved = { name: data.name ?? 'Block', properties: result, groups };
		this.#blockStructureCache.set(contentTypeKey, resolved);
		return resolved;
	}

	async #resolveDataType(
		dataTypeId: string | undefined,
	): Promise<{ editorUiAlias: string; config?: UmbPropertyEditorConfig }> {
		if (!dataTypeId) return { editorUiAlias: '' };

		const { data } = await tryExecute(this, DataTypeService.getDataTypeById({ path: { id: dataTypeId } }));
		if (!data) return { editorUiAlias: '' };

		return { editorUiAlias: data.editorUiAlias ?? '', config: data.values as UmbPropertyEditorConfig };
	}
}
