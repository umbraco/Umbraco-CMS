import { UMB_ELEMENT_WORKSPACE_CONTEXT } from '../../workspace/element-workspace.context-token.js';
import type { UmbElementDetailModel, UmbElementVariantOptionModel } from '../../types.js';
import { UmbElementVariantState } from '../../types.js';
import { UmbElementPublishingRepository } from '../repository/index.js';
import type { UmbElementVariantPublishModel } from '../types.js';
import { UMB_ELEMENT_PUBLISH_MODAL } from '../publish/constants.js';
import { UMB_ELEMENT_UNPUBLISH_MODAL } from '../unpublish/constants.js';
import { UMB_ELEMENT_SCHEDULE_MODAL } from '../schedule-publish/constants.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT } from './element-publishing.workspace-context.token.js';
import { UMB_ELEMENT_PUBLISHING_SHORTCUT_UNIQUE } from './constants.js';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPublishableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export class UmbElementPublishingWorkspaceContext extends UmbContextBase implements UmbPublishableWorkspaceContext {
	#init: Promise<unknown>;
	#elementWorkspaceContext?: typeof UMB_ELEMENT_WORKSPACE_CONTEXT.TYPE;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#publishingRepository = new UmbElementPublishingRepository(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	readonly #localize = new UmbLocalizationController(this);

	workspaceAlias = UMB_ELEMENT_WORKSPACE_ALIAS;

	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_ELEMENT_WORKSPACE_CONTEXT, async (context) => {
				if (this.#elementWorkspaceContext) {
					// remove shortcut:
					this.#elementWorkspaceContext.view.shortcuts.removeOne(UMB_ELEMENT_PUBLISHING_SHORTCUT_UNIQUE);
				}
				this.#elementWorkspaceContext = context;
				this.#elementWorkspaceContext?.view.shortcuts.addOne({
					unique: UMB_ELEMENT_PUBLISHING_SHORTCUT_UNIQUE,
					label: this.#localize.term('content_saveAndPublishShortcut'),
					key: 'p',
					modifier: true,
					action: () => this.saveAndPublish(),
				});
			})
				.asPromise({ preventTimeout: true })
				.catch(() => {
					this.#elementWorkspaceContext = undefined;
				}),

			this.consumeContext(UMB_ACTION_EVENT_CONTEXT, async (context) => {
				this.#eventContext = context;
			})
				.asPromise({ preventTimeout: true })
				.catch(() => {
					this.#eventContext = undefined;
				}),
		]);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	getEntityType() {
		return UMB_ELEMENT_ENTITY_TYPE;
	}

	public async publish() {
		throw new Error('Method not implemented. Use saveAndPublish() instead.');
	}

	/**
	 * Save and publish the element
	 * @returns {Promise<void>}
	 * @memberof UmbElementPublishingWorkspaceContext
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
	 * Unpublish the element
	 * @returns {Promise<void>}
	 * @memberof UmbElementPublishingWorkspaceContext
	 */
	public async unpublish(): Promise<void> {
		await this.#init;
		if (!this.#elementWorkspaceContext) throw new Error('Element workspace context is missing');

		const unique = this.#elementWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#elementWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		const { options, selected } = await this.#determineVariantOptions();

		// Filter to only show published variants
		const publishedOptions = options.filter(
			(option) =>
				option.variant?.state === UmbElementVariantState.PUBLISHED ||
				option.variant?.state === UmbElementVariantState.PUBLISHED_PENDING_CHANGES,
		);

		if (publishedOptions.length === 0) {
			this.#notificationContext?.peek('warning', {
				data: { message: this.#localize.term('content_itemNotPublished') },
			});
			return;
		}

		// If invariant (single culture = null), unpublish directly without modal
		if (publishedOptions.length === 1 && publishedOptions[0].culture === null) {
			const variantIds = [UmbVariantId.CreateInvariant()];
			await this.#performUnpublish(unique, entityType, variantIds);
			return;
		}

		const result = await umbOpenModal(this, UMB_ELEMENT_UNPUBLISH_MODAL, {
			data: {
				options: publishedOptions,
				pickableFilter: this.#publishableVariantsFilter,
			},
			value: { selection: selected.filter((s) => publishedOptions.some((o) => o.unique === s)) },
		}).catch(() => undefined);

		if (!result?.selection.length) return;

		const variantIds = result.selection.map((x) => UmbVariantId.FromString(x));
		await this.#performUnpublish(unique, entityType, variantIds);
	}

	async #performUnpublish(unique: string, entityType: string, variantIds: Array<UmbVariantId>) {
		const { error } = await this.#publishingRepository.unpublish(unique, variantIds);

		if (!error) {
			this.#notificationContext?.peek('positive', {
				data: { message: this.#localize.term('speechBubbles_editElementUnpublishedHeader') },
			});

			await this.#elementWorkspaceContext?.reload();

			const event = new UmbRequestReloadStructureForEntityEvent({ unique, entityType });
			this.#eventContext?.dispatchEvent(event);
		}
	}

	/**
	 * Schedule the element for publishing
	 * @returns {Promise<void>}
	 * @memberof UmbElementPublishingWorkspaceContext
	 */
	public async schedule(): Promise<void> {
		await this.#init;
		if (!this.#elementWorkspaceContext) throw new Error('Element workspace context is missing');

		const unique = this.#elementWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#elementWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		const { options, selected } = await this.#determineVariantOptions();

		const result = await umbOpenModal(this, UMB_ELEMENT_SCHEDULE_MODAL, {
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

		// Map to the correct format for the API (UmbElementVariantPublishModel)
		const variants =
			result?.selection.map<UmbElementVariantPublishModel>((x) => ({
				variantId: UmbVariantId.FromString(x.unique),
				schedule: {
					publishTime: this.#convertToDateTimeOffset(x.schedule?.publishTime),
					unpublishTime: this.#convertToDateTimeOffset(x.schedule?.unpublishTime),
				},
			})) ?? [];

		if (!variants.length) return;

		const variantIds = variants.map((x) => x.variantId);
		const saveData = await this.#elementWorkspaceContext.constructSaveData(variantIds);
		await this.#elementWorkspaceContext.runMandatoryValidationForSaveData(saveData, variantIds);
		await this.#elementWorkspaceContext.askServerToValidate(saveData, variantIds);

		return this.#elementWorkspaceContext.validateAndSubmit(
			async () => {
				if (!this.#elementWorkspaceContext) {
					throw new Error('Element workspace context is missing');
				}

				// Save the element before scheduling
				await this.#elementWorkspaceContext.performCreateOrUpdate(variantIds, saveData);

				// Schedule the element
				const { error } = await this.#publishingRepository.publish(unique, variants);
				if (error) {
					return Promise.reject(error);
				}

				const notification = { data: { message: this.#localize.term('speechBubbles_editContentScheduledSavedText') } };
				this.#notificationContext?.peek('positive', notification);

				// reload the element so all states are updated after the schedule operation
				await this.#elementWorkspaceContext.reload();

				// request reload of this entity
				const structureEvent = new UmbRequestReloadStructureForEntityEvent({ entityType, unique });
				this.#eventContext?.dispatchEvent(structureEvent);
			},
			async (reason?: unknown) => {
				this.#notificationContext?.peek('danger', {
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

	async #handleSaveAndPublish() {
		await this.#init;
		if (!this.#elementWorkspaceContext) throw new Error('Element workspace context is missing');

		const unique = this.#elementWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let variantIds: Array<UmbVariantId> = [];

		const { options, selected } = await this.#determineVariantOptions();

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the element with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else {
			// If there are multiple variants, we will open the modal to let the user pick which variants to publish.
			const result = await umbOpenModal(this, UMB_ELEMENT_PUBLISH_MODAL, {
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

		const saveData = await this.#elementWorkspaceContext.constructSaveData(variantIds);
		await this.#elementWorkspaceContext.runMandatoryValidationForSaveData(saveData, variantIds);
		await this.#elementWorkspaceContext.askServerToValidate(saveData, variantIds);

		return this.#elementWorkspaceContext.validateAndSubmit(
			async () => {
				return this.#performSaveAndPublish(variantIds, saveData);
			},
			async (reason?: unknown) => {
				// If data of the selection is not valid Then just save:
				await this.#elementWorkspaceContext!.performCreateOrUpdate(variantIds, saveData);
				// Notifying that the save was successful, but we did not publish
				this.#notificationContext?.peek('danger', {
					data: { message: this.#localize.term('speechBubbles_editContentPublishedFailedByValidation') },
				});
				return await Promise.reject(reason);
			},
		);
	}

	async #performSaveAndPublish(variantIds: Array<UmbVariantId>, saveData: UmbElementDetailModel): Promise<void> {
		await this.#init;
		if (!this.#elementWorkspaceContext) throw new Error('Element workspace context is missing');

		const unique = this.#elementWorkspaceContext.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.#elementWorkspaceContext.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		await this.#elementWorkspaceContext.performCreateOrUpdate(variantIds, saveData);

		const { error } = await this.#publishingRepository.publish(
			unique,
			variantIds.map((variantId) => ({ variantId })),
		);

		if (!error) {
			this.#notificationContext?.peek('positive', {
				data: { message: this.#localize.term('speechBubbles_editElementPublishedHeader') },
			});

			// reload the element so all states are updated after the publish operation
			await this.#elementWorkspaceContext.reload();

			const event = new UmbRequestReloadStructureForEntityEvent({ unique, entityType });
			this.#eventContext?.dispatchEvent(event);
		}
	}

	#publishableVariantsFilter = (option: UmbElementVariantOptionModel) => {
		const variantId = UmbVariantId.Create(option);
		// If the read only guard is permitted it means the variant is read only
		const isReadOnly = this.#elementWorkspaceContext!.readOnlyGuard.getIsPermittedForVariant(variantId);
		// If the variant is read only, we can't publish it
		return !isReadOnly;
	};

	async #determineVariantOptions(): Promise<{
		options: UmbElementVariantOptionModel[];
		selected: string[];
	}> {
		await this.#init;
		if (!this.#elementWorkspaceContext) throw new Error('Element workspace context is missing');

		const allOptions = await firstValueFrom(this.#elementWorkspaceContext.variantOptions);
		const options = allOptions.filter((option) => option.segment === null);

		let selected = this.#getPublishVariantsSelection();

		// Selected can contain entries that are not part of the options, therefor filter based on options.
		selected = selected.filter((x) => options.some((o) => o.unique === x));

		// Filter out read-only variants
		selected = selected.filter(
			(x) => this.#elementWorkspaceContext!.readOnlyGuard.getIsPermittedForVariant(new UmbVariantId(x)) === false,
		);

		return {
			options,
			selected,
		};
	}

	#getPublishVariantsSelection() {
		if (!this.#elementWorkspaceContext) throw new Error('Element workspace context is missing');
		const activeVariants = this.#elementWorkspaceContext.splitView.getActiveVariants();
		const activeVariantIds = activeVariants.map((x) => UmbVariantId.Create(x));
		const changedVariantIds = this.#elementWorkspaceContext.getChangedVariants();
		const activeAndChangedVariantIds = [...activeVariantIds, ...changedVariantIds];

		// if a segment has been changed, we select the "parent" culture variant
		const changedParentCultureVariantIds = activeAndChangedVariantIds
			.filter((x) => x.segment !== null)
			.map((x) => x.toSegmentInvariant());

		const selected = [...activeAndChangedVariantIds, ...changedParentCultureVariantIds].map((variantId) =>
			variantId.toString(),
		);

		return [...new Set(selected)];
	}
}

export { UmbElementPublishingWorkspaceContext as api };
