import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Permits the extension for any media item, and for the media root only when the current user has root media access.
 */
export class UmbMediaUserStartNodeAccessCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	#entityType?: string;
	#hasMediaRootAccess?: boolean;

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.hasMediaRootAccess,
				(hasMediaRootAccess) => {
					this.#hasMediaRootAccess = hasMediaRootAccess;
					this.#check();
				},
				'umbMediaRootAccessObserver',
			);
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(
				context?.entityType,
				(entityType) => {
					this.#entityType = entityType;
					this.#check();
				},
				'umbMediaEntityTypeObserver',
			);
		});
	}

	#check() {
		if (!this.#entityType) return;
		this.permitted = this.#entityType !== UMB_MEDIA_ROOT_ENTITY_TYPE || this.#hasMediaRootAccess === true;
	}
}

export { UmbMediaUserStartNodeAccessCondition as api };
