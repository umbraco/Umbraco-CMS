import { UmbUnpublishDocumentEntityAction } from '../../entity-actions/unpublish.action.js';
import { umbPickDocumentVariantModal } from '../../modals/index.js';
import type { UmbDocumentPublishingRepository } from '../../repository/index.js';
import type { UmbDocumentVariantOptionModel } from '../../types.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentUnpublishEntityBulkAction extends UmbEntityBulkActionBase<UmbDocumentPublishingRepository> {
	async execute() {
		// If there is only one selection, we can refer to the regular publish entity action:
		if (this.selection.length === 1) {
			const action = new UmbUnpublishDocumentEntityAction(this._host, '', this.selection[0], '');
			await action.execute();
			return;
		}

		if (!this.repository) throw new Error('Document publishing repository not set');

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		const options: UmbDocumentVariantOptionModel[] = (languageData?.items ?? []).map((language) => ({
			language,
			unique: new UmbVariantId(language.unique, null).toString(),
		}));

		const selectedVariants = await umbPickDocumentVariantModal(this, { type: 'unpublish', options });

		if (selectedVariants.length) {
			for (const unique of this.selection) {
				await this.repository.unpublish(unique, selectedVariants);
			}
		}
	}
}
