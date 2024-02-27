import { umbPickDocumentVariantModal } from '../modals/pick-document-variant-modal.controller.js';
import { UmbDocumentDetailRepository, UmbDocumentPublishingRepository } from '../repository/index.js';
import { UmbDocumentVariantState } from '../types.js';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<UmbDocumentPublishingRepository> {
	async execute() {
		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		// TODO: Not sure we need to use the Detail Repository for this, we might do just fine with the tree item model it self.
		const documentRepository = new UmbDocumentDetailRepository(this._host);
		const { data: documentData } = await documentRepository.requestByUnique(this.unique);

		const allOptions = (languageData?.items ?? []).map((language) => ({
			language: language,
			variant: documentData?.variants.find((variant) => variant.culture === language.unique),
			unique: new UmbVariantId(language.unique, null).toString(),
		}));

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		const options = allOptions.filter(
			(option) =>
				option.variant &&
				(option.variant.state === UmbDocumentVariantState.DRAFT ||
					option.variant.state === UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES),
		);

		// TODO: Missing features to pre-select the variant that fits with the variant-id of the tree/collection? (Again only relevant if the action is executed from a Tree or Collection) [NL]
		const selectedVariants = await umbPickDocumentVariantModal(this, { type: 'publish', options });

		if (selectedVariants.length) {
			const publishingRepository = new UmbDocumentPublishingRepository(this._host);
			await publishingRepository.publish(this.unique, selectedVariants);
		}
	}
}
