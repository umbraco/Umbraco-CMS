import type { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import { UmbBaseController, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	type ManifestCondition,
	type ManifestWithDynamicConditions,
	type UmbExtensionRegistry,
	createExtensionClass,
} from '@umbraco-cms/backoffice/extension-api';

export abstract class UmbBaseExtensionController<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions,
	SubClassType = never
> extends UmbBaseController {
	#promiseResolvers: Array<() => void> = [];
	#manifestObserver;
	#extensionRegistry: UmbExtensionRegistry<ManifestCondition>;
	#alias: string;
	#manifest?: ManifestType;
	#conditionControllers: Array<UmbExtensionCondition> = [];
	#onPermissionChanged: (isPermitted: boolean, controller: SubClassType) => void;
	#isPermitted?: boolean;

	get weight() {
		return this.#manifest?.weight ?? 0;
	}

	get permitted() {
		return this.#isPermitted ?? false;
	}

	get manifest() {
		return this.#manifest;
	}

	get alias() {
		return this.#alias;
	}

	hasConditions = async () => {
		await this.#manifestObserver.asPromise();
		return (this.#manifest?.conditions ?? []).length > 0;
	};

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestCondition>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: SubClassType) => void
	) {
		super(host, alias);
		this.#extensionRegistry = extensionRegistry;
		this.#alias = alias;
		this.#onPermissionChanged = onPermissionChanged;

		this.#manifestObserver = this.observe(
			extensionRegistry.getByAlias<ManifestType>(alias),
			async (extensionManifest) => {
				this.#isPermitted = undefined;
				this.#manifest = extensionManifest;
				if (extensionManifest) {
					this.#gotManifest(extensionManifest);
				} else {
					this.#cleanConditions();
				}
			}
		);
	}

	asPromise(): Promise<void> {
		return new Promise((resolve) => {
			this.#isPermitted === true ? resolve() : this.#promiseResolvers.push(resolve);
		});
	}

	#cleanConditions() {
		this.#conditionControllers.forEach((controller) => controller.destroy());
		this.#conditionControllers = [];
		this.removeControllerByAlias('_observeConditions');
	}

	#gotManifest(extensionManifest: ManifestType) {
		const conditionConfigs = extensionManifest.conditions ?? [];

		if (conditionConfigs.length === 0) {
			this.#cleanConditions();
			this.#onConditionsChangedCallback();
			return;
		}

		const conditionAliases = conditionConfigs
			.map((condition) => condition.alias)
			.filter((value, index, array) => array.indexOf(value) === index);

		// Clean up conditions controllers based on keepers:
		this.#conditionControllers = this.#conditionControllers.filter((current) => {
			const continueExistence = conditionConfigs.find((config) => config === current.config);
			if (!continueExistence) {
				// Destroy condition that is no longer needed.
				current.destroy();
			}
			return continueExistence;
		});

		if (conditionConfigs.length > 0) {
			// Observes the conditions and initialize as they come in.
			this.observe(
				this.#extensionRegistry.getByTypeAndAliases('condition', conditionAliases),
				async (manifests) => {
					// New comers:
					manifests.forEach((conditionManifest) => {
						const configsOfThisType = conditionConfigs.filter(
							(conditionConfig) => conditionConfig.alias === conditionManifest.alias
						);

						// Spin up conditions, based of condition configs:
						configsOfThisType.forEach(async (conditionConfig) => {
							// Check if we already have a controller for this config:
							const existing = this.#conditionControllers.find((controller) => controller.config === conditionConfig);
							if (!existing) {
								const conditionController = await createExtensionClass<UmbExtensionCondition>(conditionManifest, [
									{
										host: this,
										manifest: conditionManifest,
										config: conditionConfig,
										onChange: this.#onConditionsChangedCallback,
									},
								]);
								if (conditionController) {
									// Some how listen to it? callback/event/onChange something.
									// then call this one: this.#onConditionsChanged();
									this.#conditionControllers.push(conditionController);
									this.#onConditionsChangedCallback();
								}
							}
						});
					});
				},
				'_observeConditions'
			);
		} else {
			this.removeControllerByAlias('_observeConditions');
		}
	}

	#conditionsAreInitialized() {
		// Not good if we don't have a manifest.
		// Only good if conditions of manifest is equal to the amount of condition controllers (one for each condition).
		return (
			this.#manifest !== undefined && this.#conditionControllers.length === (this.#manifest.conditions ?? []).length
		);
	}

	#onConditionsChangedCallback = async () => {
		if (this.alias === 'Umb.SectionSidebarMenu.Media') {
			console.log('!!!!onConditionsChangedCallback', this.#isPermitted, this.#conditionControllers);
		}
		const oldValue = this.#isPermitted;
		// Find a condition that is not permitted (Notice how no conditions, means that this extension is permitted)
		const conditionsArePositive =
			this.#conditionsAreInitialized() &&
			this.#conditionControllers.find((condition) => condition.permitted === false) === undefined;

		if (conditionsArePositive) {
			if (this.#isPermitted !== true) {
				this.#isPermitted = await this._conditionsAreGood();
			}
		} else if (this.#isPermitted !== false) {
			await this._conditionsAreBad();
			this.#isPermitted = false;
		}
		if (oldValue !== this.#isPermitted) {
			if (this.#isPermitted) {
				this.#promiseResolvers.forEach((x) => x());
				this.#promiseResolvers = [];
			}
			this.#onPermissionChanged(this.#isPermitted, this as any);
		}
	};

	protected abstract _conditionsAreGood(): Promise<boolean>;

	protected abstract _conditionsAreBad(): Promise<void>;

	public equal(otherClass: UmbBaseExtensionController | undefined): boolean {
		return otherClass?.manifest === this.manifest;
	}

	public destroy(): void {
		this.#isPermitted = undefined;
		this.#promiseResolvers = [];
		this._conditionsAreBad();
		this.#onPermissionChanged(false, this as any);
		super.destroy();
		// Destroy the conditions controllers, are begin destroyed cause they are controllers.
	}
}
