import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '../../context/constants.js';
import type { MetaPropertyActionCopyToClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';

export class UmbCopyToClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionCopyToClipboardKind> {
	#propertyDatasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#clipboardContext?: typeof UMB_CLIPBOARD_PROPERTY_CONTEXT.TYPE;
	#init?: Promise<unknown>;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionCopyToClipboardKind>) {
		super(host, args);

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
				this.#propertyDatasetContext = context;
			}).asPromise({ preventTimeout: true }),

			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise({ preventTimeout: true }),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
				this.#notificationContext = context;
			}).asPromise({ preventTimeout: true }),

			this.consumeContext(UMB_CLIPBOARD_PROPERTY_CONTEXT, (context) => {
				this.#clipboardContext = context;
			}).asPromise({ preventTimeout: true }),
		]);
	}

	override async execute() {
		await this.#init;

		if (!this.#propertyDatasetContext) throw new Error('Property dataset context is not available');
		if (!this.#propertyContext) throw new Error('Property context is not available');
		if (!this.#notificationContext) throw new Error('Notification context is not available');
		if (!this.#clipboardContext) throw new Error('Clipboard context is not available');

		const propertyEditorUiAlias = this.#propertyContext.getEditorManifest()?.alias;

		if (!propertyEditorUiAlias) {
			throw new Error('Property editor alias is not available');
		}

		const workspaceName = this.#propertyDatasetContext.getName();
		const propertyLabel = this.#propertyContext.getLabel()!;
		const entryName = workspaceName ? `${workspaceName} - ${propertyLabel}` : propertyLabel;

		const propertyValue = this.#propertyContext.getValue();

		if (!propertyValue) {
			// TODO: Add correct message + localization
			this.#notificationContext!.peek('danger', { data: { message: 'The property does not have a value to copy' } });
			return;
		}

		const propertyEditorUiIcon = this.#propertyContext.getEditorManifest()?.meta.icon;

		await this.#clipboardContext.write({
			name: entryName,
			icon: propertyEditorUiIcon,
			propertyValue,
			propertyEditorUiAlias,
		});
	}
}
export { UmbCopyToClipboardPropertyAction as api };
