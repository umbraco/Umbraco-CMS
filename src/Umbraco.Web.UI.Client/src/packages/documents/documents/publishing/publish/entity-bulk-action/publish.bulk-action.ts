import { UmbDocumentPublishingRepository } from '../../index.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import type { UmbDocumentItemModel } from '../../../item/types.js';
import { UMB_DOCUMENT_PUBLISH_MODAL } from '../../../constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UmbPublishDocumentEntityAction } from '../entity-action/index.js';
import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export interface UmbBulkVariantOptions {
	allInvariant: boolean;
	options: Array<UmbDocumentVariantOptionModel>;
}

export class UmbDocumentPublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
	/**
	 * Builds variant options for bulk publish/unpublish actions based on selected documents.
	 * @param documentItems - The document items to process
	 * @param languages - Available languages in the system
	 * @returns Object containing whether all documents are invariant and the filtered variant options with document counts
	 */
	static buildVariantOptions(
		documentItems: Array<UmbDocumentItemModel>,
		languages: Array<UmbLanguageDetailModel>,
	): UmbBulkVariantOptions {
		// Check if all selected documents are invariant
		const allInvariant = documentItems.every((item) => item.variants.length === 1 && item.variants[0].culture === null);

		// Count documents per culture and count invariant documents separately
		const cultureCounts = new Map<string, number>();
		let invariantCount = 0;

		documentItems.forEach((item) => {
			// Check if this document has any culture variants
			const hasVariants = item.variants.some((variant) => variant.culture !== null);

			if (!hasVariants) {
				// This is an invariant document - it will be published under the default language
				invariantCount++;
			} else {
				// This document has culture variants - count each culture
				item.variants.forEach((variant) => {
					if (variant.culture) {
						cultureCounts.set(variant.culture, (cultureCounts.get(variant.culture) ?? 0) + 1);
					}
				});
			}
		});

		// Find the default language and add invariant documents to its count
		const defaultLanguage = languages.find((lang) => lang.isDefault);
		if (invariantCount > 0 && defaultLanguage) {
			const currentCount = cultureCounts.get(defaultLanguage.unique) ?? 0;
			cultureCounts.set(defaultLanguage.unique, currentCount + invariantCount);
		}

		// Filter options to only include languages that exist in the selected documents
		const options: Array<UmbDocumentVariantOptionModel> = languages
			.filter((language) => cultureCounts.has(language.unique))
			.map((language) => {
				const count = cultureCounts.get(language.unique) ?? 0;
				return {
					language,
					variant: {
						name: language.name,
						culture: language.unique,
						state: null,
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

	async execute() {
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) {
			throw new Error('Entity context not found');
		}
		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		const localize = new UmbLocalizationController(this);

		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Entity unique not found');

		// If there is only one selection, we can refer to the regular publish entity action:
		if (this.selection.length === 1) {
			const action = new UmbPublishDocumentEntityAction(this._host, {
				unique: this.selection[0],
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				meta: {} as never,
			});
			await action.execute();
			return;
		}

		// Fetch document items and languages in parallel
		const itemRepository = new UmbDocumentItemRepository(this._host);
		const languageRepository = new UmbLanguageCollectionRepository(this._host);

		const [{ data: documentItems }, { data: languageData }] = await Promise.all([
			itemRepository.requestItems(this.selection),
			languageRepository.requestCollection({}),
		]);

		if (!documentItems?.length) return;

		const { allInvariant, options } = UmbDocumentPublishEntityBulkAction.buildVariantOptions(
			documentItems,
			languageData?.items ?? [],
		);

		// If there is only one language available, or all selected documents are invariant, we can skip the modal and publish directly:
		if (options.length === 1 || allInvariant) {
			const confirm = await umbConfirmModal(this, {
				headline: localize.term('content_readyToPublish'),
				content: localize.term('prompt_confirmListViewPublish'),
				color: 'positive',
				confirmLabel: localize.term('actions_publish'),
			}).catch(() => false);

			if (confirm !== false) {
				// For invariant documents, use null culture; otherwise use the first language
				const variantId = allInvariant
					? UmbVariantId.CreateInvariant()
					: new UmbVariantId(options[0].language.unique, null);

				const documentCnt = await this.#publishDocuments(this.selection, [{ variantId }]);

				const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				notificationContext?.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_editContentPublishedHeader'),
						message: localize.term('speechBubbles_editMultiContentPublishedText', documentCnt),
					},
				});

				await this.#reloadChildren(entityType, unique);
			}
			return;
		}

		// Pre-select all cultures from the selected documents
		const selection: Array<string> = options.map((o) => o.unique);

		const result = await umbOpenModal(this, UMB_DOCUMENT_PUBLISH_MODAL, {
			data: {
				options,
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (variantIds.length) {
			const documentCnt = await this.#publishDocuments(
				this.selection,
				variantIds.map((variantId) => ({ variantId })),
			);

			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			notificationContext?.peek('positive', {
				data: {
					headline: localize.term('speechBubbles_editContentPublishedHeader'),
					message: localize.term(
						'speechBubbles_editMultiVariantPublishedText',
						documentCnt,
						localize.list(variantIds.map((v) => v.culture ?? '')),
					),
				},
			});

			await this.#reloadChildren(entityType, unique);
		}
	}

	async #publishDocuments(uniques: Array<string>, variants: Array<{ variantId: UmbVariantId }>): Promise<number> {
		const repository = new UmbDocumentPublishingRepository(this._host);
		let successCount = 0;
		for (const unique of uniques) {
			const { error } = await repository.publish(unique, variants);
			if (!error) successCount++;
		}
		return successCount;
	}

	async #reloadChildren(entityType: string, unique: string | null): Promise<void> {
		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) return;
		const event = new UmbRequestReloadChildrenOfEntityEvent({ entityType, unique });
		eventContext.dispatchEvent(event);
	}
}

export { UmbDocumentPublishEntityBulkAction as api };
