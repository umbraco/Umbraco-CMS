import { UmbDocumentUnpublishManifestEntityActionMeta } from '../entity-action/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../constants.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import { UmbDocumentPublishEntityBulkAction } from '../../publish/entity-bulk-action/publish.bulk-action.js';
import {
	UMB_CONTENT_UNPUBLISH_MODAL,
	UmbBulkContentPublishingController,
	UmbContentUnpublishEntityAction,
} from '@umbraco-cms/backoffice/content';
import { html, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

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
		const variantOptions = await UmbDocumentPublishEntityBulkAction.requestBulkVariantOptions(
			this._host,
			this.selection,
		);
		if (!variantOptions) return;

		const { allInvariant, options } = variantOptions;

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

		await new UmbBulkContentPublishingController(this).run({
			selection: this.selection,
			entityType,
			unique,
			headline: '#unpublish_inProgress',
			variantIds,
			labels: {
				headline: 'speechBubbles_contentUnpublished',
				multiVariant: 'speechBubbles_editMultiVariantUnpublishedText',
				multiContent: 'speechBubbles_editMultiContentUnpublishedText',
				partial: 'speechBubbles_editMultiContentUnpublishedPartialText',
			},
			process: (documentUnique) => repository.unpublish(documentUnique, variantIds),
		});
	}
}

export { UmbDocumentUnpublishEntityBulkAction as api };
