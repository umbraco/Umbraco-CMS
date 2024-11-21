import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_CLIPBOARD_CONTEXT } from '../../context/index.js';
import type { UmbClipboardEntryDetailModel } from '../../clipboard-entry/index.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../../clipboard-entry/entity.js';

export class UmbColorPickerCopyToClipboardPropertyAction extends UmbPropertyActionBase {
	#clipboardContext?: typeof UMB_CLIPBOARD_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#init?: Promise<unknown>;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<never>) {
		super(host, args);

		this.#init = Promise.all([
			this.consumeContext(UMB_CLIPBOARD_CONTEXT, (context) => {
				this.#clipboardContext = context;
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
		const propertyValue = this.#propertyContext?.getValue();
		const propertyLabel = this.#propertyContext?.getLabel() ?? 'Color';

		if (!propertyValue) {
			// TODO: Add correct message + localization
			this.#notificationContext!.peek('danger', { data: { message: 'No value' } });
		}

		// TODO: Add correct meta data
		// TODO use scaffold to create clipboard entry
		const clipboardEntryDetail: UmbClipboardEntryDetailModel = {
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			unique: UmbId.new(),
			type: 'color',
			name: propertyLabel,
			icons: ['icon-color'],
			meta: {},
			data: [propertyValue],
		};

		await this.#clipboardContext!.create(clipboardEntryDetail);

		// TODO: Add correct message + localization
		this.#notificationContext?.peek('positive', { data: { message: `${propertyLabel} copied to clipboard` } });
	}
}
export { UmbColorPickerCopyToClipboardPropertyAction as api };
