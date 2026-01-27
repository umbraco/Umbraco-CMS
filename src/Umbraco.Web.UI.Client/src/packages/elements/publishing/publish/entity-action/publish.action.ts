import type { UmbElementVariantOptionModel } from '../../../types.js';
import { UMB_ELEMENT_PUBLISH_MODAL } from '../modal/constants.js';
import { UmbElementDetailRepository } from '../../../repository/index.js';
import { UmbElementPublishingRepository } from '../../repository/index.js';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbPublishElementEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('The element unique identifier is missing');

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const localize = new UmbLocalizationController(this);

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});

		const elementRepository = new UmbElementDetailRepository(this._host);
		const { data: elementData } = await elementRepository.requestByUnique(this.args.unique);

		if (!elementData) throw new Error('The element was not found');

		const appLanguageContext = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
		if (!appLanguageContext) throw new Error('The app language context is missing');
		const appCulture = appLanguageContext.getAppCulture();

		const options: Array<UmbElementVariantOptionModel> = elementData.variants
			// only display culture variants as options
			.filter((variant) => variant.segment === null)
			.map<UmbElementVariantOptionModel>((variant) => ({
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
			}));

		// Figure out the default selections
		const selection: Array<string> = [];
		// If the app language is one of the options, select it by default:
		if (appCulture && options.some((o) => o.unique === appCulture)) {
			selection.push(new UmbVariantId(appCulture, null).toString());
		} else if (options.length > 0) {
			// If not, select the first option by default:
			selection.push(options[0].unique);
		}

		const result = await umbOpenModal(this, UMB_ELEMENT_PUBLISH_MODAL, {
			data: {
				confirmLabel: '#actions_publish',
				options,
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		// find all segments of a selected culture
		const publishableVariantIds = variantIds.flatMap((variantId) =>
			elementData.variants
				.filter((variant) => variantId.culture === variant.culture)
				.map((variant) => UmbVariantId.Create(variant).toSegment(variant.segment)),
		);

		if (publishableVariantIds.length) {
			const publishingRepository = new UmbElementPublishingRepository(this._host);
			const { error } = await publishingRepository.publish(
				this.args.unique,
				publishableVariantIds.map((variantId) => ({ variantId })),
			);

			if (error) {
				throw error;
			}

			// If the content is invariant, we need to show a different notification
			const isInvariant = options.length === 1 && options[0].culture === null;

			if (isInvariant) {
				notificationContext?.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_editContentPublishedHeader'),
						message: localize.term('speechBubbles_editContentPublishedText'),
					},
				});
			} else {
				const elementVariants = elementData.variants.filter((variant) =>
					result.selection.includes(variant.culture!),
				);
				notificationContext?.peek('positive', {
					data: {
						headline: localize.term('speechBubbles_editContentPublishedHeader'),
						message: localize.term(
							'speechBubbles_editVariantPublishedText',
							localize.list(elementVariants.map((v) => UmbVariantId.Create(v).toString() ?? v.name)),
						),
					},
				});
			}

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.args.unique,
				entityType: this.args.entityType,
			});

			actionEventContext?.dispatchEvent(event);
		}
	}
}

export default UmbPublishElementEntityAction;
