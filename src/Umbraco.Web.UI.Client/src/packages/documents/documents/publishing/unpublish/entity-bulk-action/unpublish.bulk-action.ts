import { UmbUnpublishDocumentEntityAction } from '../entity-action/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_UNPUBLISH_MODAL } from '../../../constants.js';
import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import { UmbDocumentPublishEntityBulkAction } from '../../publish/entity-bulk-action/publish.bulk-action.js';
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

		// If there is only one language available, or all selected documents are invariant, we can skip the modal and unpublish directly:
		if (options.length === 1 || allInvariant) {
			const confirm = await umbConfirmModal(this, {
				headline: localize.term('actions_unpublish'),
				content: localize.term('prompt_confirmListViewUnpublish'),
				color: 'warning',
				confirmLabel: localize.term('actions_unpublish'),
			}).catch(() => false);

			if (confirm !== false) {
				// For invariant documents, use null culture; otherwise use the first language
				const variantId = allInvariant
					? UmbVariantId.CreateInvariant()
					: new UmbVariantId(options[0].language.unique, null);

				const documentCnt = await this.#unpublishDocuments(this.selection, [variantId]);

				const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				notificationContext?.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_contentUnpublished'),
						message: localize.term('speechBubbles_editMultiContentUnpublishedText', documentCnt),
					},
				});

				await this.#reloadChildren(entityType, unique);
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

		if (variantIds.length) {
			const documentCnt = await this.#unpublishDocuments(this.selection, variantIds);

			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
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

			await this.#reloadChildren(entityType, unique);
		}
	}

	async #unpublishDocuments(uniques: Array<string>, variantIds: Array<UmbVariantId>): Promise<number> {
		const repository = new UmbDocumentPublishingRepository(this._host);
		let successCount = 0;
		for (const unique of uniques) {
			const { error } = await repository.unpublish(unique, variantIds);
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

export { UmbDocumentUnpublishEntityBulkAction as api };
