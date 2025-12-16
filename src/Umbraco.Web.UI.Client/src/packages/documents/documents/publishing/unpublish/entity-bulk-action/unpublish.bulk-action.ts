import { UmbUnpublishDocumentEntityAction } from '../entity-action/index.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_UNPUBLISH_MODAL } from '../../../constants.js';
import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export class UmbDocumentUnpublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) {
			throw new Error('Entity context not found');
		}
		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const localize = new UmbLocalizationController(this);

		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Entity unique not found');

		// If there is only one selection, we can refer to the regular unpublish entity action:
		if (this.selection.length === 1) {
			const action = new UmbUnpublishDocumentEntityAction(this._host, {
				unique: this.selection[0],
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				meta: {} as never,
			});
			await action.execute();
			return;
		}

		// Fetch document items to check their variants
		const itemRepository = new UmbDocumentItemRepository(this._host);
		const { data: documentItems } = await itemRepository.requestItems(this.selection);

		// Check if all selected documents are invariant
		const allInvariant = documentItems?.every(
			(item) => item.variants.length === 1 && item.variants[0].culture === null,
		);

		// Count documents per culture (excluding null for invariant)
		const cultureCounts = new Map<string, number>();
		documentItems?.forEach((item) => {
			item.variants.forEach((variant) => {
				if (variant.culture) {
					cultureCounts.set(variant.culture, (cultureCounts.get(variant.culture) ?? 0) + 1);
				}
			});
		});

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		// Filter options to only include languages that exist in the selected documents
		const options = (languageData?.items ?? [])
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

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Event context not found');
		}
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType,
			unique,
		});

		// If there is only one language available, or all selected documents are invariant, we can skip the modal and unpublish directly:
		if (options.length === 1 || allInvariant) {
			const localizationController = new UmbLocalizationController(this._host);
			const confirm = await umbConfirmModal(this, {
				headline: localizationController.term('actions_unpublish'),
				content: localizationController.term('prompt_confirmListViewUnpublish'),
				color: 'warning',
				confirmLabel: localizationController.term('actions_unpublish'),
			}).catch(() => false);

			if (confirm !== false) {
				// For invariant documents, use null culture; otherwise use the first language
				const variantId = allInvariant
					? UmbVariantId.CreateInvariant()
					: new UmbVariantId(options[0].language.unique, null);
				const publishingRepository = new UmbDocumentPublishingRepository(this._host);
				let documentCnt = 0;

				for (let i = 0; i < this.selection.length; i++) {
					const id = this.selection[i];
					const { error } = await publishingRepository.unpublish(id, [variantId]);

					if (!error) {
						documentCnt++;
					}
				}

				notificationContext?.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_contentUnpublished'),
						message: localize.term('speechBubbles_editMultiContentUnpublishedText', documentCnt),
					},
				});

				eventContext.dispatchEvent(event);
			}
			return;
		}

		// Pre-select all cultures from the selected documents
		const selection: Array<string> = options.map((o) => o.unique);

		const result = await umbOpenModal(this, UMB_DOCUMENT_UNPUBLISH_MODAL, {
			data: {
				options,
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		const repository = new UmbDocumentPublishingRepository(this._host);

		if (variantIds.length) {
			let documentCnt = 0;
			for (const unique of this.selection) {
				const { error } = await repository.unpublish(unique, variantIds);

				if (!error) {
					documentCnt++;
				}
			}

			notificationContext?.peek('positive', {
				data: {
					headline: localize.term('speechBubbles_contentUnpublished'),
					message: localize.term(
						'speechBubbles_editMultiVariantUnpublishedText',
						documentCnt,
						localize.list(variantIds.map((v) => v.culture ?? '')),
					),
				},
			});

			eventContext.dispatchEvent(event);
		}
	}
}

export { UmbDocumentUnpublishEntityBulkAction as api };
