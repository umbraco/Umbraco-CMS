import { UmbUnpublishDocumentEntityAction } from '../../entity-actions/unpublish.action.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import type { UmbDocumentVariantOptionModel } from '../../types.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentUnpublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		// If there is only one selection, we can refer to the regular publish entity action:
		if (this.selection.length === 1) {
			const action = new UmbUnpublishDocumentEntityAction(this._host, {
				unique: this.selection[0],
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				meta: {},
			});
			await action.execute();
			return;
		}

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		const options: UmbDocumentVariantOptionModel[] = (languageData?.items ?? []).map((language) => ({
			language,
			unique: new UmbVariantId(language.unique, null).toString(),
			culture: language.unique,
			segment: null,
		}));

		const selectedVariants = await umbPickDocumentVariantModal(this, { type: 'unpublish', options });
		const repository = new UmbDocumentPublishingRepository(this._host);

		if (selectedVariants.length) {
			for (const unique of this.selection) {
				await repository.unpublish(unique, selectedVariants);
			}
		}
	}
}
