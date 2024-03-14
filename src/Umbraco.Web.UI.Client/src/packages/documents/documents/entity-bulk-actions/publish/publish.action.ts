import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import { UmbPublishDocumentEntityAction } from '../../entity-actions/publish.action.js';
import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import { UMB_DOCUMENT_PUBLISH_MODAL } from '../../modals/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentPublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		// If there is only one selection, we can refer to the regular publish entity action:
		if (this.selection.length === 1) {
			const action = new UmbPublishDocumentEntityAction(this._host, {
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
			variant: {
				name: language.name,
				culture: language.unique,
				state: null,
				createDate: null,
				publishDate: null,
				updateDate: null,
				segment: null,
			},
			unique: new UmbVariantId(language.unique, null).toString(),
			culture: language.unique,
			segment: null,
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

		const repository = new UmbDocumentPublishingRepository(this._host);

		if (variantIds.length) {
			for (const unique of this.selection) {
				await repository.publish(
					unique,
					variantIds.map((variantId) => ({ variantId })),
				);
			}
		}
	}
}
