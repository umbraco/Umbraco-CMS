import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../../constants.js';
import type { UmbDataTypeItemRepository } from '../../../repository/index.js';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbConditionBase, createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDataTypeAllowDeleteActionCondition extends UmbConditionBase<never> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<never>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;

			this.observe(
				context.unique,
				async (unique) => {
					if (!unique) {
						this.permitted = true; // Default to allowed if no unique
						return;
					}
					await this.#checkItemDeletable(unique);
				},
				'_entityContextObserver',
			);
		});
	}

	async #checkItemDeletable(unique: string) {
		const repository = await createExtensionApiByAlias<UmbDataTypeItemRepository>(
			this,
			UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
		);
		const { data } = await repository.requestItems([unique]);
		const item = data?.[0];
		this.permitted = item?.isDeletable !== false;
	}
}

export { UmbDataTypeAllowDeleteActionCondition as api };
