import { UMB_DOCUMENT_PUBLISH_MODAL } from '../modals/publish-modal/index.js';
import { UmbDocumentDetailRepository, UmbDocumentPublishingRepository } from '../repository/index.js';
import type { UmbDocumentVariantOptionModel } from '../types.js';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<unknown> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<unknown>) {
		super(host, args);
	}

	async execute() {
		if (!this.args.unique) throw new Error('The document unique identifier is missing');

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		const documentRepository = new UmbDocumentDetailRepository(this._host);
		const { data: documentData } = await documentRepository.requestByUnique(this.args.unique);

		if (!documentData) throw new Error('The document was not found');

		// If the document has only one variant, we can skip the modal and publish directly:
		if (documentData.variants.length === 1) {
			const variantId = UmbVariantId.Create(documentData.variants[0]);
			const publishingRepository = new UmbDocumentPublishingRepository(this._host);
			await publishingRepository.publish(this.args.unique, [variantId]);
			return;
		}

		const options: Array<UmbDocumentVariantOptionModel> = (languageData?.items ?? []).map((language) => ({
			culture: language.unique,
			segment: null,
			language: language,
			variant: documentData.variants.find((variant) => variant.culture === language.unique),
			unique: new UmbVariantId(language.unique, null).toString(),
		}));

		// Figure out the default selections
		// TODO: Missing features to pre-select the variant that fits with the variant-id of the tree/collection? (Again only relevant if the action is executed from a Tree or Collection) [NL]
		const selection: Array<string> = [];
		const context = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
		const appCulture = context.getAppCulture();
		// If the app language is one of the options, select it by default:
		if (appCulture && options.some((o) => o.unique === appCulture)) {
			selection.push(new UmbVariantId(appCulture, null).toString());
		}

		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const result = await modalManagerContext
			.open(this, UMB_DOCUMENT_PUBLISH_MODAL, {
				data: {
					options,
				},
				value: { selection },
			})
			.onSubmit()
			.catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (variantIds.length) {
			const publishingRepository = new UmbDocumentPublishingRepository(this._host);
			await publishingRepository.publish(this.args.unique, variantIds);
		}
	}
}
export default UmbPublishDocumentEntityAction;
