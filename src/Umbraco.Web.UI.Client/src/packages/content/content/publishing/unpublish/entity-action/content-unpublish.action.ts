import { UMB_CONTENT_UNPUBLISH_MODAL } from '../modal/constants.js';
import type { UmbContentDetailModel } from '../../../types.js';
import type { MetaEntityActionContentUnpublishKind, UmbContentUnpublishingRepository } from './types.js';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbContentUnpublishEntityAction extends UmbEntityActionBase<MetaEntityActionContentUnpublishKind> {
	override async execute() {
		await this.executeWithResult();
	}

	/**
	 * Runs the unpublish flow and reports whether an unpublish actually occurred.
	 * @returns {Promise<boolean>} `true` if the entity was unpublished, `false` if the user cancelled or made no selection.
	 * @memberof UmbContentUnpublishEntityAction
	 */
	public async executeWithResult(): Promise<boolean> {
		if (!this.args.unique) throw new Error('The unique identifier is missing');

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const localize = new UmbLocalizationController(this);

		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestAllItems();

		const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<UmbContentDetailModel>>(
			this,
			this.args.meta.detailRepositoryAlias,
		);
		const { data: detailData } = await detailRepository.requestByUnique(this.args.unique);

		if (!detailData) throw new Error('The item was not found');

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

		const cultureVariantOptions = detailData.variants.filter((variant) => variant.segment === null);

		const options: Array<UmbEntityVariantOptionModel> = cultureVariantOptions.map<UmbEntityVariantOptionModel>(
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

		// Figure out the default selection:
		const selection: Array<string> = [];
		// If the app language is one of the options, select it by default:
		if (appCulture && options.some((o) => o.unique === appCulture)) {
			selection.push(new UmbVariantId(appCulture, null).toString());
		} else {
			// If not, select the first option by default:
			selection.push(options[0].unique);
		}

		const result = await umbOpenModal(this, UMB_CONTENT_UNPUBLISH_MODAL, {
			data: {
				unique: this.args.unique,
				options,
				pickableFilter: (option) => {
					if (!option.culture) return false;
					if (currentUserHasAccessToAllLanguages) return true;
					return currentUserAllowedLanguages.includes(option.culture);
				},
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
				configurationRepositoryAlias: this.args.meta.configurationRepositoryAlias,
			},
			value: { selection },
		}).catch(() => undefined);

		if (!result?.selection.length) return false;

		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (!variantIds.length) return false;

		const publishingRepository = await createExtensionApiByAlias<UmbContentUnpublishingRepository>(
			this,
			this.args.meta.publishingRepositoryAlias,
		);
		const { error } = await publishingRepository.unpublish(this.args.unique, variantIds);

		if (error) {
			throw error;
		}

		notificationContext?.peek('positive', {
			data: {
				message: localize.string(
					this.args.meta.unpublishedNotificationMessage || '#speechBubbles_editContentUnpublishedHeader',
				),
			},
		});

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext?.dispatchEvent(event);

		return true;
	}
}

export { UmbContentUnpublishEntityAction as api };
