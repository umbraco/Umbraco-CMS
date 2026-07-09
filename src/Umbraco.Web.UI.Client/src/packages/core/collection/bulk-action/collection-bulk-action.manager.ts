import type { UmbCollectionBulkActionConfiguration } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

const observerAlias = 'umbCollectionBulkActionsObserver';

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

	#config?: UmbCollectionBulkActionConfiguration;

	/**
	 * Sets the configuration for the bulk action manager.
	 * @param {UmbCollectionBulkActionConfiguration | undefined} config - Configuration for the bulk action manager.
	 */
	setConfig(config: UmbCollectionBulkActionConfiguration | undefined) {
		const oldConfig = this.#config;
		this.#config = config;
		this.#toggleFeature(oldConfig, config);
	}

	/**
	 * Gets the current configuration.
	 * @returns {UmbCollectionBulkActionConfiguration | undefined} The current configuration.
	 */
	getConfig(): UmbCollectionBulkActionConfiguration | undefined {
		return this.#config;
	}

	#toggleFeature(
		oldConfig: UmbCollectionBulkActionConfiguration | undefined,
		newConfig: UmbCollectionBulkActionConfiguration | undefined,
	) {
		// Handle enabling/disabling bulk actions based on the new configuration.
		// Only disable if explicitly set to false.
		if (newConfig?.enabled === false) {
			// Avoid unnecessary operations if bulk actions are already disabled.
			if (oldConfig?.enabled === false && newConfig?.enabled === false) return;
			this.removeUmbControllerByAlias(observerAlias);
			this.#hasBulkActions.setValue(false);
		} else {
			// Avoid unnecessary operations if bulk actions are already enabled.
			if (oldConfig?.enabled === true && newConfig?.enabled === true) return;
			this.#observeBulkActions();
		}
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
			observerAlias,
		);
	}
}
