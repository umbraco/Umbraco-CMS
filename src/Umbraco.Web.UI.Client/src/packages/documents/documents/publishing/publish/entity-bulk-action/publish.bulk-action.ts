import { UmbDocumentPublishingRepository } from '../../index.js';
import { UMB_DOCUMENT_PUBLISH_MODAL } from '../../../constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UmbPublishDocumentEntityAction } from '../entity-action/index.js';
import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export class UmbDocumentPublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
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

		// If there is only one language available, or all selected documents are invariant, we can skip the modal and publish directly:
		if (options.length === 1 || allInvariant) {
			const localizationController = new UmbLocalizationController(this._host);
			const confirm = await umbConfirmModal(this, {
				headline: localizationController.term('content_readyToPublish'),
				content: localizationController.term('prompt_confirmListViewPublish'),
				color: 'positive',
				confirmLabel: localizationController.term('actions_publish'),
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
					const { error } = await publishingRepository.publish(id, [{ variantId }]);

					if (!error) {
						documentCnt++;
					}
				}

				notificationContext?.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_editContentPublishedHeader'),
						message: localize.term('speechBubbles_editMultiContentPublishedText', documentCnt),
					},
				});

				eventContext.dispatchEvent(event);
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

		const repository = new UmbDocumentPublishingRepository(this._host);

		if (variantIds.length) {
			let documentCnt = 0;
			for (const unique of this.selection) {
				const { error } = await repository.publish(
					unique,
					variantIds.map((variantId) => ({ variantId })),
				);

				if (!error) {
					documentCnt++;
				}
			}

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

			eventContext.dispatchEvent(event);
		}
	}
}

export { UmbDocumentPublishEntityBulkAction as api };
