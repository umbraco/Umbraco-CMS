import { UmbClipboardEntryDetailRepository } from '../../clipboard-entry/index.js';
import type { UmbClipboardCopyResolver } from '../../resolver/types.js';
import type { MetaPropertyActionCopyToClipboardKind } from './types.js';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';

export class UmbCopyToClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionCopyToClipboardKind> {
	#propertyDatasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#init?: Promise<unknown>;

	#copyResolverAlias?: string;
	#copyResolver?: UmbClipboardCopyResolver;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionCopyToClipboardKind>) {
		super(host, args);

		this.#copyResolverAlias = args.meta.clipboardCopyResolverAlias;

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

		if (this.#copyResolverAlias) {
			new UmbExtensionApiInitializer(
				this,
				umbExtensionsRegistry,
				this.#copyResolverAlias,
				[this],
				(permitted, ctrl) => {
					this.#copyResolver = permitted ? (ctrl.api as UmbClipboardCopyResolver) : undefined;
				},
			);
		}
	}

	override async execute() {
		await this.#init;
		const workspaceName = this.#propertyDatasetContext?.getName() || 'Unnamed workspace';
		const propertyLabel = this.#propertyContext?.getLabel() || 'Unnamed property';
		const propertyValue = this.#propertyContext?.getValue();
		const entryName = workspaceName ? `${workspaceName} - ${propertyLabel}` : propertyLabel;

		if (!propertyValue) {
			// TODO: Add correct message + localization
			this.#notificationContext!.peek('danger', { data: { message: 'The property does not have a value to copy' } });
			return;
		}

		if (!this.#copyResolver) {
			throw new Error('The copy resolver is not initialized');
		}

		await this.#copyResolver.copy(propertyValue, entryName);
	}
}
export { UmbCopyToClipboardPropertyAction as api };
