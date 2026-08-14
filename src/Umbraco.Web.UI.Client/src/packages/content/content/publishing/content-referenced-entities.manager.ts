import type { UmbEntityPublishAwarenessApi } from './entity-publish-awareness.extension.js';
import { UmbPropertyValueEntityReferencesController } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { createExtensionApiByAlias, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

/**
 * Resolves the entities directly referenced by a set of property values (e.g. the values of a document or
 * element being published) that need attention before publishing — that is, entities of a type that has
 * registered an {@link import('./entity-publish-awareness.extension.js').ManifestEntityPublishAwareness}, and
 * whose {@link UmbEntityPublishAwarenessApi.needsAttention} check returns `true` for them.
 * @exports
 * @class UmbContentReferencedEntitiesManager
 * @augments {UmbControllerBase}
 */
export class UmbContentReferencedEntitiesManager extends UmbControllerBase {
	/**
	 * Resolves the referenced entities across the given property values that currently need attention.
	 * @param {Array<UmbPropertyValueDataPotentiallyWithEditorAlias>} values - The property values to resolve references from.
	 * @returns {Promise<Array<UmbEntityModel>>} The referenced entities needing attention, sorted by name then unique.
	 */
	async getEntitiesNeedingAttention(
		values: Array<UmbPropertyValueDataPotentiallyWithEditorAlias>,
	): Promise<Array<UmbEntityModel>> {
		const references = await this.#resolveReferences(values);
		if (!references.length) return [];

		const groupedByEntityType = this.#groupByEntityType(references);

		const results = await Promise.all(
			Object.entries(groupedByEntityType).map(([entityType, entities]) =>
				this.#resolveNeedingAttention(entityType, entities),
			),
		);

		return results.flat().sort(this.#compare);
	}

	async #resolveReferences(
		values: Array<UmbPropertyValueDataPotentiallyWithEditorAlias>,
	): Promise<Array<UmbEntityModel>> {
		const referencesPerValue = await Promise.all(
			values.map((value) => new UmbPropertyValueEntityReferencesController(this).resolve(value)),
		);
		return this.#dedupe(referencesPerValue.flat());
	}

	#dedupe(entities: Array<UmbEntityModel>): Array<UmbEntityModel> {
		const seen = new Set<string>();
		const result: Array<UmbEntityModel> = [];
		for (const entity of entities) {
			const key = `${entity.entityType}:${entity.unique}`;
			if (seen.has(key)) continue;
			seen.add(key);
			result.push(entity);
		}
		return result;
	}

	#groupByEntityType(entities: Array<UmbEntityModel>): Record<string, Array<UmbEntityModel>> {
		const groups: Record<string, Array<UmbEntityModel>> = {};
		for (const entity of entities) {
			(groups[entity.entityType] ??= []).push(entity);
		}
		return groups;
	}

	async #resolveNeedingAttention(entityType: string, entities: Array<UmbEntityModel>): Promise<Array<UmbEntityModel>> {
		// No manifest means this entity type hasn't opted in to publish awareness — nothing to check.
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'entityPublishAwareness',
			(x) => x.forEntityTypes.includes(entityType),
		)[0];
		if (!manifest) return [];

		const api = await createExtensionApi<UmbEntityPublishAwarenessApi>(this, manifest);
		if (!api) return [];

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			manifest.meta.itemRepositoryAlias,
		);

		const uniques = entities.map((x) => x.unique).filter((x): x is string => !!x);
		if (!uniques.length) return [];

		const { data: items } = await itemRepository.requestItems(uniques);
		return (items ?? []).filter((item) => api.needsAttention(item));
	}

	readonly #compare = (a: UmbEntityModel, b: UmbEntityModel): number => {
		const nameCompare = this.#getSortName(a).localeCompare(this.#getSortName(b));
		if (nameCompare !== 0) return nameCompare;
		return (a.unique ?? '').localeCompare(b.unique ?? '');
	};

	// Entities are variant-aware (name lives per-variant), but this manager only deals in invariant identity —
	// falls back to a flat `name` for any entity type whose item model isn't variant-shaped.
	#getSortName(entity: UmbEntityModel): string {
		const item = entity as { variants?: Array<{ name?: string }>; name?: string };
		return item.variants?.[0]?.name ?? item.name ?? '';
	}
}
