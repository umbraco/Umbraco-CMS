import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/document-workspace.context-token.js';
import type {
	UmbDocumentDetailModel,
	UmbDocumentVariantOptionModel,
	UmbDocumentVariantPublishModel,
} from '../../types.js';
import { UmbDocumentPublishingRepository } from '../repository/index.js';
import { UmbDocumentPublishedPendingChangesManager } from '../pending-changes/index.js';
import { UMB_DOCUMENT_SCHEDULE_MODAL } from '../schedule-publish/constants.js';
import { UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL } from '../publish-with-descendants/constants.js';
import { UMB_DOCUMENT_PUBLISH_MODAL } from '../publish/constants.js';
import { UmbUnpublishDocumentEntityAction } from '../unpublish/index.js';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from './document-publishing.workspace-context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbDocumentPublishingWorkspaceContext extends UmbContextBase {
	/**
	 * Manages the pending changes for the published document.
	 * @memberof UmbDocumentPublishingWorkspaceContext
	 */
	public readonly publishedPendingChanges = new UmbDocumentPublishedPendingChangesManager(this);

	#init: Promise<unknown>;
	#documentWorkspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#publishingRepository = new UmbDocumentPublishingRepository(this);
	#publishedDocumentData?: UmbDocumentDetailModel;
	#currentUnique?: UmbEntityUnique;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	readonly #localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, async (context) => {
				this.#documentWorkspaceContext = context;
				this.#initPendingChanges();
			})
				.asPromise({ preventTimeout: true })
				.catch(() => {
					// If the context is not available, we can assume that the context is not available.
					this.#documentWorkspaceContext = undefined;
				}),

			this.consumeContext(UMB_ACTION_EVENT_CONTEXT, async (context) => {
				this.#eventContext = context;
			})
				.asPromise({ preventTimeout: true })
				.catch(() => {
					// If the context is not available, we can assume that the context is not available.
					this.#eventContext = undefined;
				}),
		]);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	public async publish() {
		throw new Error('Method not implemented.');
	}

	/**
	 * Save and publish the document
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPublishingWorkspaceContext
	 */
	public async saveAndPublish(): Promise<void> {
		const elementStyle = (this.getHostElement() as HTMLElement).style;
		elementStyle.removeProperty('--uui-color-invalid');
		elementStyle.removeProperty('--uui-color-invalid-emphasis');
		elementStyle.removeProperty('--uui-color-invalid-standalone');
		elementStyle.removeProperty('--uui-color-invalid-contrast');
		return this.#handleSaveAndPublish();
	}

	/**
	 * Schedule the document for publishing
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPublishingWorkspaceContext
	 */
	public async schedule(): Promise<void> {
		await this.#init;
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const unique = this.#documentWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#documentWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		const { options, selected } = await this.#determineVariantOptions();

		const result = await umbOpenModal(this, UMB_DOCUMENT_SCHEDULE_MODAL, {
			data: {
				options,
				activeVariants: selected,
				pickableFilter: this.#publishableVariantsFilter,
				prevalues: options.map((option) => ({
					unique: option.unique,
					schedule: {
						publishTime: option.variant?.scheduledPublishDate,
						unpublishTime: option.variant?.scheduledUnpublishDate,
					},
				})),
			},
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to the correct format for the API (UmbDocumentVariantPublishModel)
		const variants =
			result?.selection.map<UmbDocumentVariantPublishModel>((x) => ({
				variantId: UmbVariantId.FromString(x.unique),
				schedule: {
					publishTime: this.#convertToDateTimeOffset(x.schedule?.publishTime),
					unpublishTime: this.#convertToDateTimeOffset(x.schedule?.unpublishTime),
				},
			})) ?? [];

		if (!variants.length) return;

		const variantIds = variants.map((x) => x.variantId);
		const saveData = await this.#documentWorkspaceContext.constructSaveData(variantIds);
		await this.#documentWorkspaceContext.runMandatoryValidationForSaveData(saveData);
		await this.#documentWorkspaceContext.askServerToValidate(saveData, variantIds);

		// TODO: Only validate the specified selection.. [NL]
		return this.#documentWorkspaceContext.validateAndSubmit(
			async () => {
				if (!this.#documentWorkspaceContext) {
					throw new Error('Document workspace context is missing');
				}

				// Save the document before scheduling
				await this.#documentWorkspaceContext.performCreateOrUpdate(variantIds, saveData);

				// Schedule the document
				const { error } = await this.#publishingRepository.publish(unique, variants);
				if (error) {
					return Promise.reject(error);
				}

				const notification = { data: { message: this.#localize.term('speechBubbles_editContentScheduledSavedText') } };
				this.#notificationContext?.peek('positive', notification);

				// reload the document so all states are updated after the publish operation
				await this.#documentWorkspaceContext.reload();
				this.#loadAndProcessLastPublished();

				// request reload of this entity
				const structureEvent = new UmbRequestReloadStructureForEntityEvent({ entityType, unique });
				this.#eventContext?.dispatchEvent(structureEvent);
			},
			async (reason?: any) => {
				const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				if (!notificationContext) {
					throw new Error('Notification context is missing');
				}
				notificationContext.peek('danger', {
					data: { message: this.#localize.term('speechBubbles_editContentScheduledNotSavedText') },
				});

				return Promise.reject(reason);
			},
		);
	}

	/**
	 * Convert a date string to a server time string in ISO format, example: 2021-01-01T12:00:00.000+00:00.
	 * The input must be a valid date string, otherwise it will return null.
	 * The output matches the DateTimeOffset format in C#.
	 * @param dateString
	 */
	#convertToDateTimeOffset(dateString: string | null | undefined) {
		if (!dateString || dateString.length === 0) {
			return null;
		}

		const date = new Date(dateString);

		if (isNaN(date.getTime())) {
			console.warn(`[Schedule]: Invalid date: ${dateString}`);
			return null;
		}

		// Convert the date to UTC time in ISO format before sending it to the server
		return date.toISOString();
	}

	/**
	 * Publish the document with descendants
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPublishingWorkspaceContext
	 */
	public async publishWithDescendants(): Promise<void> {
		await this.#init;
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const unique = this.#documentWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#documentWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		const { options, selected } = await this.#determineVariantOptions();

		const result = await umbOpenModal(this, UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL, {
			data: {
				options,
				pickableFilter: this.#publishableVariantsFilter,
			},
			value: { selection: selected },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to variantIds
		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (!variantIds.length) return;

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const localize = new UmbLocalizationController(this);

		const primaryVariantName = await this.observe(this.#documentWorkspaceContext.name(variantIds[0])).asPromise();

		const waitNotice = notificationContext?.peek('warning', {
			data: {
				headline: localize.term('publish_publishAll', primaryVariantName),
				message: localize.term('publish_inProgress'),
			},
		});

		const { error } = await this.#publishingRepository.publishWithDescendants(
			unique,
			variantIds,
			result.includeUnpublishedDescendants ?? false,
		);

		waitNotice?.close();

		if (!error) {
			notificationContext?.peek('positive', {
				data: {
					message: localize.term('publish_nodePublishAll', primaryVariantName),
				},
			});

			// reload the document so all states are updated after the publish operation
			await this.#documentWorkspaceContext.reload();
			this.#loadAndProcessLastPublished();

			// request reload of this entity
			const structureEvent = new UmbRequestReloadStructureForEntityEvent({ entityType, unique });
			this.#eventContext?.dispatchEvent(structureEvent);

			// request reload of the children
			const childrenEvent = new UmbRequestReloadChildrenOfEntityEvent({ entityType, unique });
			this.#eventContext?.dispatchEvent(childrenEvent);
		}
	}

	/**
	 * Unpublish the document
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPublishingWorkspaceContext
	 */
	public async unpublish(): Promise<void> {
		await this.#init;
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const unique = this.#documentWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#documentWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		// TODO: remove meta
		await new UmbUnpublishDocumentEntityAction(this, { unique, entityType, meta: {} as never }).execute();
	}

	async #handleSaveAndPublish() {
		await this.#init;
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const unique = this.#documentWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let variantIds: Array<UmbVariantId> = [];

		const { options, selected } = await this.#determineVariantOptions();

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else {
			// If there are multiple variants, we will open the modal to let the user pick which variants to publish.
			const result = await umbOpenModal(this, UMB_DOCUMENT_PUBLISH_MODAL, {
				data: {
					headline: this.#localize.term('content_saveAndPublishModalTitle'),
					options,
					pickableFilter: this.#publishableVariantsFilter,
				},
				value: { selection: selected },
			}).catch(() => undefined);

			if (!result?.selection.length || !unique) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		}

		const saveData = await this.#documentWorkspaceContext.constructSaveData(variantIds);
		await this.#documentWorkspaceContext.runMandatoryValidationForSaveData(saveData, variantIds);
		await this.#documentWorkspaceContext.askServerToValidate(saveData, variantIds);

		// TODO: Only validate the specified selection.. [NL]
		return this.#documentWorkspaceContext.validateAndSubmit(
			async () => {
				return this.#performSaveAndPublish(variantIds, saveData);
			},
			async (reason?: any) => {
				// If data of the selection is not valid Then just save:
				await this.#documentWorkspaceContext!.performCreateOrUpdate(variantIds, saveData);
				// Notifying that the save was successful, but we did not publish, which is what we want to symbolize here. [NL]
				const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				if (!notificationContext) {
					throw new Error('Notification context is missing');
				}
				// TODO: Get rid of the save notification.
				notificationContext.peek('danger', {
					data: { message: this.#localize.term('speechBubbles_editContentPublishedFailedByValidation') },
				});
				// Reject even thought the save was successful, but we did not publish, which is what we want to symbolize here. [NL]
				return await Promise.reject(reason);
			},
		);
	}

	async #performSaveAndPublish(variantIds: Array<UmbVariantId>, saveData: UmbDocumentDetailModel): Promise<void> {
		await this.#init;
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const unique = this.#documentWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#documentWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		await this.#documentWorkspaceContext.performCreateOrUpdate(variantIds, saveData);

		const { error } = await this.#publishingRepository.publish(
			unique,
			variantIds.map((variantId) => ({ variantId })),
		);

		if (!error) {
			this.#notificationContext?.peek('positive', {
				data: {
					message: this.#localize.term('speechBubbles_editContentPublishedHeader'),
				},
			});

			// reload the document so all states are updated after the publish operation
			await this.#documentWorkspaceContext.reload();
			this.#loadAndProcessLastPublished();

			const event = new UmbRequestReloadStructureForEntityEvent({ unique, entityType });
			this.#eventContext?.dispatchEvent(event);
		}
	}

	#publishableVariantsFilter = (option: UmbDocumentVariantOptionModel) => {
		const variantId = UmbVariantId.Create(option);
		// If the read only guard is permitted it means the variant is read only
		const isReadOnly = this.#documentWorkspaceContext!.readOnlyGuard.getIsPermittedForVariant(variantId);
		// If the variant is read only, we can't publish it
		const isPublishable = !isReadOnly;
		return isPublishable;
	};

	async #determineVariantOptions(): Promise<{
		options: UmbDocumentVariantOptionModel[];
		selected: string[];
	}> {
		await this.#init;
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const allOptions = await firstValueFrom(this.#documentWorkspaceContext.variantOptions);
		const options = allOptions.filter((option) => option.segment === null);

		// TODO: this is a temporary copy of the content-detail workspace context method.
		// we need to implement custom selection that makes sense for each the publishing modal.
		let selected = this.#getPublishVariantsSelection();

		// Selected can contain entries that are not part of the options, therefor the modal filters selection based on options.
		selected = selected.filter((x) => options.some((o) => o.unique === x));

		// Filter out read-only variants
		// TODO: This would not work with segments, as the 'selected'-array is an array of strings, not UmbVariantId's. [NL]
		// Please have a look at the implementation in the content-detail workspace context, as that one compares variantIds. [NL]
		selected = selected.filter(
			(x) => this.#documentWorkspaceContext!.readOnlyGuard.getIsPermittedForVariant(new UmbVariantId(x)) === false,
		);

		return {
			options,
			selected,
		};
	}

	#getPublishVariantsSelection() {
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');
		const activeVariants = this.#documentWorkspaceContext.splitView.getActiveVariants();
		const activeVariantIds = activeVariants.map((x) => UmbVariantId.Create(x));
		const changedVariantIds = this.#documentWorkspaceContext.getChangedVariants();
		const activeAndChangedVariantIds = [...activeVariantIds, ...changedVariantIds];

		// if a segment has been changed, we select the "parent" culture variant as it is currently only possible to select between cultures in the dialogs
		const changedParentCultureVariantIds = activeAndChangedVariantIds
			.filter((x) => x.segment !== null)
			.map((x) => x.toSegmentInvariant());

		const selected = [...activeAndChangedVariantIds, ...changedParentCultureVariantIds].map((variantId) =>
			variantId.toString(),
		);

		const uniqueSelected = [...new Set(selected)];

		return uniqueSelected;
	}

	async #initPendingChanges() {
		if (!this.#documentWorkspaceContext) {
			// Do not complain in this case.
			return;
		}
		this.observe(
			observeMultiple([this.#documentWorkspaceContext.unique, this.#documentWorkspaceContext.isNew]),
			([unique, isNew]) => {
				// We have loaded in a new document, so we need to clear the states
				if (unique !== this.#currentUnique) {
					this.#clear();
				}

				this.#currentUnique = unique;

				if (isNew === false && unique) {
					this.#loadAndProcessLastPublished();
				}
			},
			'uniqueObserver',
		);

		this.observe(
			this.#documentWorkspaceContext.persistedData,
			() => this.#processPendingChanges(),
			'umbPersistedDataObserver',
		);
	}

	#hasPublishedVariant() {
		const variants = this.#documentWorkspaceContext?.getVariants();
		return (
			variants?.some(
				(variant) =>
					variant.state === DocumentVariantStateModel.PUBLISHED ||
					variant.state === DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES,
			) ?? false
		);
	}

	async #loadAndProcessLastPublished() {
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		// No need to check pending changes for new documents
		if (this.#documentWorkspaceContext.getIsNew()) return;

		const unique = this.#documentWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		// Only load the published data if the document is already published or has been published before
		const hasPublishedVariant = this.#hasPublishedVariant();
		if (!hasPublishedVariant) return;

		const { data } = await this.#publishingRepository.published(unique);
		this.#publishedDocumentData = data;
		this.#processPendingChanges();
	}

	#processPendingChanges() {
		if (!this.#documentWorkspaceContext) throw new Error('Document workspace context is missing');

		const persistedData = this.#documentWorkspaceContext.getPersistedData();
		const publishedData = this.#publishedDocumentData;
		if (!persistedData || !publishedData) return;

		this.publishedPendingChanges.process({ persistedData, publishedData });
	}

	#clear() {
		this.#publishedDocumentData = undefined;
		this.publishedPendingChanges.clear();
	}
}

export { UmbDocumentPublishingWorkspaceContext as api };
