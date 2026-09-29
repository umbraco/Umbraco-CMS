import { UMB_CONTENT_PUBLISH_MODAL } from '../modal/constants.js';
import type { UmbContentDetailModel } from '../../../types.js';
import type { MetaEntityActionContentPublishKind, UmbContentPublishingRepository } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

/**
 * @description - Shared entity action for publishing content-like entities (Document, Element, ...).
 * Resolves its detail/publishing repositories by alias from `meta`, so a single implementation can serve every
 * consumer registered against the `contentPublish` kind.
 * @exports
 * @class UmbContentPublishEntityAction
 * @augments UmbEntityActionBase
 */
export class UmbContentPublishEntityAction extends UmbEntityActionBase<MetaEntityActionContentPublishKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('The unique identifier is missing');
		const unique = this.args.unique;

		const { languageData, detailData } = await this.#resolveDetailData(unique);
		const { appCulture, currentUserAllowedLanguages, currentUserHasAccessToAllLanguages } =
			await this.#resolveCurrentUserLanguageAccess();

		const options = this.#buildVariantOptions(detailData, languageData, appCulture);
		const selection = this.#determineDefaultSelection(options, appCulture);

		const result = await umbOpenModal(this, UMB_CONTENT_PUBLISH_MODAL, {
			data: {
				confirmLabel: '#actions_publish',
				options,
				pickableFilter: (option) =>
					this.#isLanguagePickable(option, currentUserAllowedLanguages, currentUserHasAccessToAllLanguages),
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result.selection.map((x) => UmbVariantId.FromString(x));
		const publishableVariantIds = this.#resolvePublishableVariantIds(detailData, variantIds);

		if (!publishableVariantIds.length) return;

		await this.#publish(unique, publishableVariantIds);
		await this.#notifyPublished(options, detailData, result.selection);
		await this.#dispatchReloadEvent(unique);
	}

	async #resolveDetailData(unique: string): Promise<{
		languageData: { items: Array<UmbLanguageDetailModel> } | undefined;
		detailData: UmbContentDetailModel;
	}> {
		if (!this.args.meta.detailRepositoryAlias) throw new Error('The detail repository alias is missing in meta');

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestAllItems();

		const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<UmbContentDetailModel>>(
			this,
			this.args.meta.detailRepositoryAlias,
		);
		const { data: detailData } = await detailRepository.requestByUnique(unique);

		if (!detailData) throw new Error('The item was not found');

		return { languageData, detailData };
	}

	async #resolveCurrentUserLanguageAccess(): Promise<{
		appCulture: string | undefined;
		currentUserAllowedLanguages: Array<string>;
		currentUserHasAccessToAllLanguages: boolean;
	}> {
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

		return { appCulture, currentUserAllowedLanguages, currentUserHasAccessToAllLanguages };
	}

	#buildVariantOptions(
		detailData: UmbContentDetailModel,
		languageData: { items: Array<UmbLanguageDetailModel> } | undefined,
		appCulture: string | undefined,
	): Array<UmbEntityVariantOptionModel> {
		// only display culture variants as options
		const cultureVariantOptions = detailData.variants.filter((variant) => variant.segment === null);

		return cultureVariantOptions.map<UmbEntityVariantOptionModel>((variant) => ({
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
	}

	#determineDefaultSelection(
		options: Array<UmbEntityVariantOptionModel>,
		appCulture: string | undefined,
	): Array<string> {
		if (!options.length) throw new Error('No variants are available to publish');

		// If the app language is one of the options, select it by default:
		if (appCulture && options.some((o) => o.unique === appCulture)) {
			return [new UmbVariantId(appCulture, null).toString()];
		}

		// If not, select the first option by default:
		return [options[0].unique];
	}

	#isLanguagePickable(
		option: UmbEntityVariantOptionModel,
		currentUserAllowedLanguages: Array<string>,
		currentUserHasAccessToAllLanguages: boolean,
	): boolean {
		if (!option.culture) return false;
		if (currentUserHasAccessToAllLanguages) return true;
		return currentUserAllowedLanguages.includes(option.culture);
	}

	#resolvePublishableVariantIds(
		detailData: UmbContentDetailModel,
		variantIds: Array<UmbVariantId>,
	): Array<UmbVariantId> {
		// find all segments of a selected culture
		return variantIds.flatMap((variantId) =>
			detailData.variants
				.filter((variant) => variantId.culture === variant.culture)
				.map((variant) => UmbVariantId.Create(variant).toSegment(variant.segment)),
		);
	}

	async #publish(unique: string, publishableVariantIds: Array<UmbVariantId>): Promise<void> {
		if (!this.args.meta.publishingRepositoryAlias)
			throw new Error('The publishing repository alias is missing in meta');

		const publishingRepository = await createExtensionApiByAlias<UmbContentPublishingRepository>(
			this,
			this.args.meta.publishingRepositoryAlias,
		);
		const { error } = await publishingRepository.publish(
			unique,
			publishableVariantIds.map((variantId) => ({ variantId })),
		);

		if (error) {
			throw error;
		}
	}

	async #notifyPublished(
		options: Array<UmbEntityVariantOptionModel>,
		detailData: UmbContentDetailModel,
		selection: Array<string>,
	): Promise<void> {
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const localize = new UmbLocalizationController(this);

		// If the content is invariant, we need to show a different notification
		const isInvariant = options.length === 1 && options[0].culture === null;

		if (isInvariant) {
			notificationContext?.peek('positive', {
				data: {
					headline: localize.term('speechBubbles_editContentPublishedHeader'),
					message: localize.string(
						this.args.meta.publishedNotificationMessage || '#speechBubbles_editContentPublishedText',
					),
				},
			});
		} else {
			const publishedVariants = detailData.variants.filter((variant) => selection.includes(variant.culture!));
			notificationContext?.peek('positive', {
				data: {
					headline: localize.term('speechBubbles_editContentPublishedHeader'),
					message: localize.string(
						this.args.meta.variantPublishedNotificationMessage || '#speechBubbles_editVariantPublishedText',
						localize.list(publishedVariants.map((v) => UmbVariantId.Create(v).toString() ?? v.name)),
					),
				},
			});
		}
	}

	async #dispatchReloadEvent(unique: string): Promise<void> {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique,
			entityType: this.args.entityType,
		});

		actionEventContext?.dispatchEvent(event);
	}
}

export { UmbContentPublishEntityAction as api };
