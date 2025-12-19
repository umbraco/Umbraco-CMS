import { UmbUnpublishDocumentEntityAction } from '../entity-action/unpublish.action.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_UNPUBLISH_MODAL } from '../../../constants.js';
import { UmbDocumentPublishingRepository } from '../../repository/document-publishing.repository.js';
import {
	buildVariantOptions,
	processDocumentsInBatches,
	showBulkResultNotification,
} from '../../bulk-publish.utils.js';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export class UmbDocumentUnpublishEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		// If there is only one selection, delegate to the regular unpublish action
		if (this.selection.length === 1) {
			const action = new UmbUnpublishDocumentEntityAction(this._host, {
				unique: this.selection[0],
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				meta: {} as never,
			});
			await action.execute();
			return;
		}

		// Fetch contexts in parallel
		const [entityContext, notificationContext, eventContext, appLanguageContext] = await Promise.all([
			this.getContext(UMB_ENTITY_CONTEXT),
			this.getContext(UMB_NOTIFICATION_CONTEXT),
			this.getContext(UMB_ACTION_EVENT_CONTEXT),
			this.getContext(UMB_APP_LANGUAGE_CONTEXT),
		]);

		if (!entityContext) throw new Error('Entity context not found');
		if (!eventContext) throw new Error('Event context not found');
		if (!appLanguageContext) throw new Error('App language context not found');

		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Entity unique not found');

		const localize = new UmbLocalizationController(this);
		const repository = new UmbDocumentPublishingRepository(this._host);

		// Fetch available languages and build variant options
		const languageRepository = new UmbLanguageCollectionRepository(this._host);
		const { data: languageData } = await languageRepository.requestCollection({});
		const options = buildVariantOptions(languageData?.items ?? []);

		let variantIds: Array<UmbVariantId>;

		// Single language: show confirm dialog
		if (options.length === 1) {
			const confirmed = await umbConfirmModal(this, {
				headline: localize.term('actions_unpublish'),
				content: localize.term('prompt_confirmListViewUnpublish'),
				color: 'warning',
				confirmLabel: localize.term('actions_unpublish'),
			}).catch(() => false);

			if (confirmed === false) return;

			variantIds = [new UmbVariantId(options[0].language.unique, null)];
		} else {
			// Multiple languages: show variant picker modal
			const appCulture = appLanguageContext.getAppCulture();
			const preselection: Array<string> = [];

			if (appCulture && options.some((o) => o.unique === appCulture)) {
				preselection.push(new UmbVariantId(appCulture, null).toString());
			}

			const result = await umbOpenModal(this, UMB_DOCUMENT_UNPUBLISH_MODAL, {
				data: { options },
				value: { selection: preselection },
			}).catch(() => undefined);

			if (!result?.selection.length) return;

			variantIds = result.selection.map((x) => UmbVariantId.FromString(x));
		}

		// Unpublish documents
		const { succeeded, failed } = await processDocumentsInBatches(
			this.selection,
			(unique) => repository.unpublish(unique, variantIds),
			notificationContext,
			localize,
			localize.term('speechBubbles_contentUnpublished'),
		);

		// Show result notification
		showBulkResultNotification(
			notificationContext,
			localize,
			'unpublish',
			succeeded,
			failed,
			this.selection.length,
			variantIds,
		);

		// Reload children
		eventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent({ entityType, unique }));
	}
}

export { UmbDocumentUnpublishEntityBulkAction as api };
