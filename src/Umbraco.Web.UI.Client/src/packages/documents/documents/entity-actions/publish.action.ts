import { umbPickDocumentVariantModal } from '../modals/pick-document-variant-modal.controller.js';
import { UmbDocumentDetailRepository, UmbDocumentPublishingRepository } from '../repository/index.js';
import { UmbDocumentVariantState } from '../types.js';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<unknown> {
	async execute() {
		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		const documentRepository = new UmbDocumentDetailRepository(this._host);
		const { data: documentData } = await documentRepository.requestByUnique(this.unique);

		if (!documentData) throw new Error('The document was not found');

		// If the document has only one variant, we can skip the modal and publish directly:
		if (documentData.variants.length === 1) {
			const variantId = UmbVariantId.Create(documentData.variants[0]);
			const publishingRepository = new UmbDocumentPublishingRepository(this._host);
			await publishingRepository.publish(this.unique, [variantId]);
			return;
		}

		const allOptions = (languageData?.items ?? []).map((language) => ({
			culture: language.unique,
			segment: null,
			language: language,
			variant: documentData.variants.find((variant) => variant.culture === language.unique),
			unique: new UmbVariantId(language.unique, null).toString(),
		}));

		// TODO: Maybe move this to modal [NL]
		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		const options = allOptions.filter(
			(option) =>
				option.variant &&
				(option.variant.state === UmbDocumentVariantState.DRAFT ||
					option.variant.state === UmbDocumentVariantState.PUBLISHED ||
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
