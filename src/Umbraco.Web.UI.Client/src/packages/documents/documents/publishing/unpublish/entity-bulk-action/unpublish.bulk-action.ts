import { UmbDocumentUnpublishManifestEntityActionMeta } from '../entity-action/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../constants.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import { showBulkPublishingResultNotification } from '../../bulk-publishing-result-notification.js';
import { UmbDocumentPublishEntityBulkAction } from '../../publish/entity-bulk-action/publish.bulk-action.js';
import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import { UMB_CONTENT_UNPUBLISH_MODAL, UmbContentUnpublishEntityAction } from '@umbraco-cms/backoffice/content';
import { html, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UmbEntityBulkActionBase,
	UmbEntityBulkActionProgressController,
} from '@umbraco-cms/backoffice/entity-bulk-action';
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
		if (!entityContext) throw new Error('Entity context not found');

		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();
		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Entity unique not found');

		// If there is only one selection, we can refer to the regular unpublish entity action:
		if (this.selection.length === 1) {
			return this.#unpublishSingleSelection();
		}

		return this.#unpublishMultipleSelections(entityType, unique);
	}

	async #unpublishSingleSelection(): Promise<void> {
		const action = new UmbContentUnpublishEntityAction(this._host, {
			unique: this.selection[0],
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			meta: UmbDocumentUnpublishManifestEntityActionMeta,
		});
		await action.execute();
	}

	async #unpublishMultipleSelections(entityType: string, unique: string | null): Promise<void> {
		// Fetch document items and languages in parallel
		const itemRepository = new UmbDocumentItemRepository(this._host);
		const languageRepository = new UmbLanguageCollectionRepository(this._host);

		const [{ data: documentItems }, { data: languageData }] = await Promise.all([
			itemRepository.requestItems(this.selection),
			languageRepository.requestAllItems(),
		]);

		if (!documentItems?.length) return;

		const { allInvariant, options } = UmbDocumentPublishEntityBulkAction.buildVariantOptions(
			documentItems,
			languageData?.items ?? [],
		);

		// If there is only one language available, or all selected documents are invariant, we can skip the modal and unpublish directly:
		if (options.length === 1 || allInvariant) {
			return this.#unpublishSingleVariant(entityType, unique, options, allInvariant);
		}

		return this.#unpublishSelectedVariants(entityType, unique, options);
	}

	async #unpublishSingleVariant(
		entityType: string,
		unique: string | null,
		options: Array<UmbDocumentVariantOptionModel>,
		allInvariant: boolean,
	): Promise<void> {
		const confirm = await umbConfirmModal(this, {
			headline: '#actions_unpublish',
			content: '#prompt_confirmListViewUnpublish',
			color: 'warning',
			confirmLabel: '#actions_unpublish',
		}).catch(() => false);

		if (confirm === false) return;

		// For invariant documents, use null culture; otherwise use the first language
		const variantId = allInvariant
			? UmbVariantId.CreateInvariant()
			: new UmbVariantId(options[0].language.unique, null);

		await this.#bulkUnpublish([variantId], entityType, unique);
	}

	async #unpublishSelectedVariants(
		entityType: string,
		unique: string | null,
		options: Array<UmbDocumentVariantOptionModel>,
	): Promise<void> {
		// Pre-select all cultures from the selected documents
		const selection: Array<string> = options.map((o) => o.unique);

		const result = await umbOpenModal(this, UMB_CONTENT_UNPUBLISH_MODAL, {
			data: {
				options,
				renderAdditionalLabel: UmbDocumentUnpublishEntityBulkAction.#renderDocumentCountLabel,
			},
			value: { selection },
		}).catch(() => undefined);

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		if (!variantIds.length) return;

		await this.#bulkUnpublish(variantIds, entityType, unique);
	}

	static #renderDocumentCountLabel(option: UmbEntityVariantOptionModel) {
		const documentCount = (option as UmbDocumentVariantOptionModel).documentCount;
		return documentCount !== undefined
			? html`<div class="label-status">
					<umb-localize key="general_documentCount" .args=${[documentCount]}> ${documentCount} documents </umb-localize>
				</div>`
			: nothing;
	}

	// Unpublishes the selection sequentially in a progress dialog, then reports the outcome and reloads.
	async #bulkUnpublish(variantIds: Array<UmbVariantId>, entityType: string, unique: string | null): Promise<void> {
		const repository = new UmbDocumentPublishingRepository(this._host);
		const localize = new UmbLocalizationController(this);

		const result = await new UmbEntityBulkActionProgressController(this).runWithProgress({
			headline: localize.term('unpublish_inProgress'),
			uniques: this.selection,
			process: (documentUnique) => repository.unpublish(documentUnique, variantIds),
		});

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		showBulkPublishingResultNotification(notificationContext, localize, 'unpublish', {
			succeeded: result.succeeded,
			total: this.selection.length,
			variantIds,
		});

		if (result.succeeded > 0) {
			await this.#reloadChildren(entityType, unique);
		}
	}

	async #reloadChildren(entityType: string, unique: string | null): Promise<void> {
		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) return;
		const event = new UmbRequestReloadChildrenOfEntityEvent({ entityType, unique });
		eventContext.dispatchEvent(event);
	}
}

export { UmbDocumentUnpublishEntityBulkAction as api };
