import { UmbDocumentPublishingRepository } from '../../index.js';
import { UmbBulkDocumentPublishingController } from '../../bulk-publishing.controller.js';
import { UmbDocumentVariantState } from '../../../variant-state.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import type { UmbDocumentItemModel } from '../../../item/types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UmbDocumentPublishManifestEntityActionMeta } from '../entity-action/constants.js';
import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import { UMB_CONTENT_PUBLISH_MODAL, UmbContentPublishEntityAction } from '@umbraco-cms/backoffice/content';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbBulkVariantOptions {
	allInvariant: boolean;
	options: Array<UmbDocumentVariantOptionModel>;
}

export class UmbDocumentPublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
	/**
	 * Builds variant options for bulk publish/unpublish actions based on selected documents.
	 * @param documentItems - The document items to process
	 * @param languages - Available languages in the system
	 * @returns Object containing whether all documents are invariant and the filtered variant options with document counts and states
	 */
	static buildVariantOptions(
		documentItems: Array<UmbDocumentItemModel>,
		languages: Array<UmbLanguageDetailModel>,
	): UmbBulkVariantOptions {
		// Check if all selected documents are invariant
		const allInvariant = documentItems.every((item) => item.variants.length === 1 && item.variants[0].culture === null);

		// Count documents per culture, track states, and count invariant documents separately
		const cultureCounts = new Map<string, number>();
		const cultureStates = new Map<string, Array<UmbDocumentVariantState | null>>();
		let invariantCount = 0;
		const invariantStates: Array<UmbDocumentVariantState | null> = [];

		documentItems.forEach((item) => {
			// Check if this document has any culture variants
			const hasVariants = item.variants.some((variant) => variant.culture !== null);

			if (!hasVariants) {
				// This is an invariant document - it will be published under the default language
				invariantCount++;
				// Get the state from the invariant variant
				const invariantVariant = item.variants.find((v) => v.culture === null);
				if (invariantVariant) {
					invariantStates.push(invariantVariant.state);
				}
			} else {
				// This document has culture variants - count each culture
				item.variants.forEach((variant) => {
					if (variant.culture) {
						cultureCounts.set(variant.culture, (cultureCounts.get(variant.culture) ?? 0) + 1);
						// Track the state for this culture
						const states = cultureStates.get(variant.culture) ?? [];
						states.push(variant.state);
						cultureStates.set(variant.culture, states);
					}
				});
			}
		});

		// Find the default language and add invariant documents to its count
		const defaultLanguage = languages.find((lang) => lang.isDefault);
		if (invariantCount > 0 && defaultLanguage) {
			const currentCount = cultureCounts.get(defaultLanguage.unique) ?? 0;
			cultureCounts.set(defaultLanguage.unique, currentCount + invariantCount);
			// Add invariant states to the default language's states
			const existingStates = cultureStates.get(defaultLanguage.unique) ?? [];
			cultureStates.set(defaultLanguage.unique, [...existingStates, ...invariantStates]);
		}

		// Filter options to only include languages that exist in the selected documents
		const options: Array<UmbDocumentVariantOptionModel> = languages
			.filter((language) => cultureCounts.has(language.unique))
			.map((language) => {
				const count = cultureCounts.get(language.unique) ?? 0;
				const states = cultureStates.get(language.unique) ?? [];
				// Determine representative state: if all same, show that; otherwise show DRAFT (Unpublished)
				const representativeState = UmbDocumentPublishEntityBulkAction.#determineRepresentativeState(states);

				return {
					language,
					variant: {
						name: language.name,
						culture: language.unique,
						state: representativeState,
						createDate: null,
						publishDate: null,
						updateDate: null,
						segment: null,
						scheduledPublishDate: null,
						scheduledUnpublishDate: null,
						flags: [],
					},
					unique: new UmbVariantId(language.unique, null).toString(),
					culture: language.unique,
					segment: null,
					documentCount: count,
				};
			});

		return { allInvariant, options };
	}

	/**
	 * Determines a representative state from an array of variant states.
	 * If all states are the same, returns that state.
	 * If states differ, returns DRAFT (displays as "Unpublished") since mixed states can't be meaningfully represented
	 * and DRAFT is more accurate than showing "Not created" (which shouldn't appear since those variants are filtered out).
	 * @param states
	 */
	static #determineRepresentativeState(states: Array<UmbDocumentVariantState | null>): UmbDocumentVariantState | null {
		if (states.length === 0) return null;

		// If all states are the same, return that state; otherwise return DRAFT (shown as "Unpublished")
		const uniqueStates = new Set(states);
		return uniqueStates.size === 1 ? states[0] : UmbDocumentVariantState.DRAFT;
	}

	/**
	 * Fetches the selected documents and the available languages, then builds the variant options for a
	 * bulk publish/unpublish. Returns `undefined` when none of the selected documents could be loaded.
	 * @param host - The controller host used to resolve the repositories.
	 * @param selection - The uniques of the selected documents.
	 */
	static async requestBulkVariantOptions(
		host: UmbControllerHost,
		selection: Array<string>,
	): Promise<UmbBulkVariantOptions | undefined> {
		const itemRepository = new UmbDocumentItemRepository(host);
		const languageRepository = new UmbLanguageCollectionRepository(host);

		const [{ data: documentItems }, { data: languageData }] = await Promise.all([
			itemRepository.requestItems(selection),
			languageRepository.requestAllItems(),
		]);

		if (!documentItems?.length) return undefined;

		return UmbDocumentPublishEntityBulkAction.buildVariantOptions(documentItems, languageData?.items ?? []);
	}

	async execute() {
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) {
			throw new Error('Entity context not found');
		}
		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Entity unique not found');

		// If there is only one selection, we can refer to the regular publish entity action:
		if (this.selection.length === 1) {
			const action = new UmbContentPublishEntityAction(this._host, {
				unique: this.selection[0],
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				meta: UmbDocumentPublishManifestEntityActionMeta,
			});
			await action.execute();
			return;
		}

		const variantOptions = await UmbDocumentPublishEntityBulkAction.requestBulkVariantOptions(
			this._host,
			this.selection,
		);
		if (!variantOptions) return;

		const { allInvariant, options } = variantOptions;

		// If there is only one language available, or all selected documents are invariant, we can skip the modal and publish directly:
		if (options.length === 1 || allInvariant) {
			const confirm = await umbConfirmModal(this, {
				headline: '#content_readyToPublish',
				content: '#prompt_confirmListViewPublish',
				color: 'positive',
				confirmLabel: '#actions_publish',
			}).catch(() => false);

			if (confirm !== false) {
				// For invariant documents, use null culture; otherwise use the first language
				const variantId = allInvariant
					? UmbVariantId.CreateInvariant()
					: new UmbVariantId(options[0].language.unique, null);

				await this.#bulkPublish([{ variantId }], [variantId], entityType, unique);
			}
			return;
		}

		// Pre-select all cultures from the selected documents
		const selection: Array<string> = options.map((o) => o.unique);

		const result = await umbOpenModal(this, UMB_CONTENT_PUBLISH_MODAL, {
			data: {
				options,
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (variantIds.length) {
			await this.#bulkPublish(
				variantIds.map((variantId) => ({ variantId })),
				variantIds,
				entityType,
				unique,
			);
		}
	}

	// Publishes the selection sequentially in a progress dialog, then reports the outcome and reloads.
	async #bulkPublish(
		variants: Array<{ variantId: UmbVariantId }>,
		variantIds: Array<UmbVariantId>,
		entityType: string,
		unique: string | null,
	): Promise<void> {
		const repository = new UmbDocumentPublishingRepository(this._host);

		await new UmbBulkDocumentPublishingController(this).run({
			selection: this.selection,
			entityType,
			unique,
			type: 'publish',
			headlineKey: 'publish_inProgress',
			variantIds,
			process: (documentUnique) => repository.publish(documentUnique, variants),
		});
	}
}

export { UmbDocumentPublishEntityBulkAction as api };
