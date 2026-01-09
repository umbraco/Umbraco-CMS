import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Manager responsible for tracking the availability of bulk actions in a collection.
 */
export class UmbCollectionBulkActionManager extends UmbControllerBase {
	#hasBulkActions = new UmbBooleanState<undefined>(undefined);

	/**
	 * Observable that emits `true` if bulk actions are available, `false` if none are registered,
	 * or `undefined` if the state has not yet been determined.
	 */
	public readonly hasBulkActions = this.#hasBulkActions.asObservable();

	/**
	 * Creates a new instance of the bulk action manager.
	 * @param {UmbControllerHost} host - The controller host that owns this manager.
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#observeBulkActions();
	}

	#observeBulkActions() {
		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'entityBulkAction',
			null,
			(bulkActionControllers) => {
				this.#hasBulkActions.setValue(bulkActionControllers.length > 0);
			},
		);
	}
}
