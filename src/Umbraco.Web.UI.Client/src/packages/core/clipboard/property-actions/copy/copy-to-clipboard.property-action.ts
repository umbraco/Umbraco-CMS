import { UmbClipboardEntryDetailRepository } from '../../clipboard-entry/index.js';
import type { MetaPropertyActionCopyToClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';

export class UmbColorPickerCopyToClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionCopyToClipboardKind> {
	#propertyDatasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#init?: Promise<unknown>;

	#clipboardDetailRepository = new UmbClipboardEntryDetailRepository(this);

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<never>) {
		super(host, args);

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
				this.#propertyDatasetContext = context;
			}).asPromise(),

			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
				this.#notificationContext = context;
			}).asPromise(),
		]);
	}

	override async execute() {
		await this.#init;
		const workspaceName = this.#propertyDatasetContext?.getName() || 'Unnamed workspace';
		const propertyLabel = this.#propertyContext?.getLabel() || 'Unnamed property';
		const propertyValue = this.#propertyContext?.getValue();
		const clipboardEntryName = workspaceName ? `${workspaceName} - ${propertyLabel}` : propertyLabel;

		if (!propertyValue) {
			// TODO: Add correct message + localization
			this.#notificationContext!.peek('danger', { data: { message: 'The property does not have a value to copy' } });
			return;
		}

		// TODO: Add correct meta data
		const { data } = await this.#clipboardDetailRepository.createScaffold({
			type: 'color', // TODO: make this value dynamic
			name: clipboardEntryName,
			icons: ['icon-color'], // TODO: make this value dynamic
			meta: {},
			data: [propertyValue],
		});

		if (data) {
			await this.#clipboardDetailRepository.create(data);
		}
	}
}
export { UmbColorPickerCopyToClipboardPropertyAction as api };
