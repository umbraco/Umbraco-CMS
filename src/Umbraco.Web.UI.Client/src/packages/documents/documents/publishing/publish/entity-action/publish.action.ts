import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import { UMB_DOCUMENT_PUBLISH_MODAL } from '../modal/constants.js';
import { UmbDocumentDetailRepository } from '../../../repository/index.js';
import { UmbDocumentPublishingRepository } from '../../repository/index.js';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('The document unique identifier is missing');

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!notificationContext) throw new Error('The notification context is missing');
		const localize = new UmbLocalizationController(this);

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		const documentRepository = new UmbDocumentDetailRepository(this._host);
		const { data: documentData } = await documentRepository.requestByUnique(this.args.unique);

		if (!documentData) throw new Error('The document was not found');

		const appLanguageContext = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
		if (!appLanguageContext) throw new Error('The app language context is missing');
		const appCulture = appLanguageContext.getAppCulture();

		const currentUserContext = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		if (!currentUserContext) throw new Error('The current user context is missing');
		const currentUserAllowedLanguages = currentUserContext.getLanguages();
		const currentUserHasAccessToAllLanguages = currentUserContext.getHasAccessToAllLanguages();

		if (currentUserAllowedLanguages === undefined) throw new Error('The current user languages are missing');
		if (currentUserHasAccessToAllLanguages === undefined)
			throw new Error('The current user access to all languages is missing');

		const options: Array<UmbDocumentVariantOptionModel> = documentData.variants.map<UmbDocumentVariantOptionModel>(
			(variant) => ({
				culture: variant.culture,
				segment: variant.segment,
				language: languageData?.items.find((language) => language.unique === variant.culture) ?? {
					name: appCulture!,
					entityType: 'language',
					fallbackIsoCode: null,
					isDefault: true,
					isMandatory: false,
					unique: appCulture!,
				},
				variant,
				unique: new UmbVariantId(variant.culture, variant.segment).toString(),
			}),
		);

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) throw new Error('The action event context is missing');
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		// If the document has only one variant, we can skip the modal and publish directly:
		if (options.length === 1) {
			const variantId = UmbVariantId.Create(documentData.variants[0]);
			const publishingRepository = new UmbDocumentPublishingRepository(this._host);
			const { error } = await publishingRepository.publish(this.args.unique, [{ variantId }]);
			if (!error) {
				notificationContext.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_editContentPublishedHeader'),
						message: localize.term('speechBubbles_editContentPublishedText'),
					},
				});
			}
			actionEventContext.dispatchEvent(event);
			return;
		}

		// Figure out the default selections
		// TODO: Missing features to pre-select the variant that fits with the variant-id of the tree/collection? (Again only relevant if the action is executed from a Tree or Collection) [NL]
		const selection: Array<string> = [];
		// If the app language is one of the options, select it by default:
		if (appCulture && options.some((o) => o.unique === appCulture)) {
			selection.push(new UmbVariantId(appCulture, null).toString());
		} else {
			// If not, select the first option by default:
			selection.push(options[0].unique);
		}

		const result = await umbOpenModal(this, UMB_DOCUMENT_PUBLISH_MODAL, {
			data: {
				options,
				pickableFilter: (option) => {
					if (!option.culture) return false;
					if (currentUserHasAccessToAllLanguages) return true;
					return currentUserAllowedLanguages.includes(option.culture);
				},
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (variantIds.length) {
			const publishingRepository = new UmbDocumentPublishingRepository(this._host);
			const { error } = await publishingRepository.publish(
				this.args.unique,
				variantIds.map((variantId) => ({ variantId })),
			);

			if (!error) {
				const documentVariants = documentData.variants.filter((variant) => result.selection.includes(variant.culture!));
				notificationContext.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_editContentPublishedHeader'),
						message: localize.term(
							'speechBubbles_editVariantPublishedText',
							localize.list(documentVariants.map((v) => v.culture ?? v.name)),
						),
					},
				});
			}

			actionEventContext.dispatchEvent(event);
		}
	}
}
export default UmbPublishDocumentEntityAction;
