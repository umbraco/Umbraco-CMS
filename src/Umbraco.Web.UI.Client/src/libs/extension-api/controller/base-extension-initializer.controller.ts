import { createExtensionApi } from '../functions/index.js';
import type { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbConditionConfigBase,
	ManifestCondition,
	ManifestWithDynamicConditions,
	UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';
import { jsonStringComparison, type UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

const observeConditionsCtrlAlias = Symbol();
const observeExtensionsCtrlAlias = Symbol();

/**
 * This abstract Controller holds the core to manage a single Extension.
 * When the extension is permitted to be used, then the extender of this class can instantiate what is relevant for this type and thereby make it available for the consumer.
 * @abstract
 * @class UmbBaseExtensionInitializer
 */
export abstract class UmbBaseExtensionInitializer<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions,
	SubClassType = never,
> extends UmbControllerBase {
	//
	#promiseResolvers: Array<() => void> = [];
	#manifestObserver!: UmbObserverController<ManifestType | undefined>;
	#extensionRegistry: UmbExtensionRegistry<ManifestCondition>;
	#alias: string;
	#overwrites: Array<string> = [];
	#manifest?: ManifestType;
	#conditionControllers: Array<UmbExtensionCondition> = [];
	#onPermissionChanged?: (isPermitted: boolean, controller: SubClassType) => void;
	protected _isConditionsPositive?: boolean;
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

	get overwrites() {
		return this.#overwrites;
	}

	hasConditions = async () => {
		await this.#manifestObserver.asPromise();
		return (this.#manifest?.conditions ?? []).length > 0;
	};

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestCondition>,
		controllerTypeName: string,
		alias: string,
		onPermissionChanged?: (isPermitted: boolean, controller: SubClassType) => void,
	) {
		super(host, controllerTypeName + '_' + alias);
		this.#extensionRegistry = extensionRegistry;
		this.#alias = alias;
		this.#onPermissionChanged = onPermissionChanged;
	}
	protected _init() {
		this.#manifestObserver = this.observe(
			this.#extensionRegistry.byAlias<ManifestType>(this.#alias),
			(extensionManifest) => {
				if (extensionManifest) {
					if (extensionManifest.overwrites) {
						if (typeof extensionManifest.overwrites === 'string') {
							this.#overwrites = [extensionManifest.overwrites];
						} else if (Array.isArray(extensionManifest.overwrites)) {
							this.#overwrites = extensionManifest.overwrites;
						}
					}
					this.#gotManifest(extensionManifest);
				} else {
					this.#manifest = undefined;
					this.#clearPermittedState();
					this.#overwrites = [];
					this.#cleanConditions();
				}
			},
			observeExtensionsCtrlAlias,
		);
	}

	asPromise(): Promise<void> {
		return new Promise((resolve) => {
			if (this.#isPermitted === true) {
				resolve();
			} else {
				this.#promiseResolvers.push(resolve);
			}
		});
	}

	#cleanConditions() {
		if (this.#conditionControllers === undefined || this.#conditionControllers.length === 0) return;
		this.#conditionControllers.forEach((controller) => controller.destroy());
		this.#conditionControllers = [];
		this.removeUmbControllerByAlias(observeConditionsCtrlAlias);
	}

	#gotManifest(manifest: ManifestType) {
		const conditionConfigs = manifest.conditions ?? [];
		const oldManifest = this.#manifest;
		this.#manifest = manifest;

		/*
		// As conditionConfigs might have been configured as something else than an array, then we ignorer them. [NL]
		if (conditionConfigs.length === 0) {
			this.#cleanConditions();
			this.#onConditionsChangedCallback();
			return;
		}
		*/

		const conditionAliases = conditionConfigs
			.map((condition) => condition.alias)
			.filter((value, index, array) => array.indexOf(value) === index);

		const oldAmountOfControllers = this.#conditionControllers.length;
		// Clean up conditions controllers based on keepers:
		this.#conditionControllers = this.#conditionControllers.filter((current) => {
			const continueExistence = conditionConfigs.find((config) => config === current.config);
			if (!continueExistence) {
				// Destroy condition that is no longer needed. [NL]
				current.destroy();
			}
			return continueExistence;
		});

		if (conditionConfigs.length > 0) {
			// Observes the conditions and initialize as they come in. [NL]
			this.observe(
				this.#extensionRegistry.byTypeAndAliases('condition', conditionAliases),
				this.#gotConditions,
				observeConditionsCtrlAlias,
			);
		} else {
			this.removeUmbControllerByAlias(observeConditionsCtrlAlias);
		}

		// If permitted we want to fire an update because we got a new manifest. [NL]
		if (this.#isPermitted) {
			// Check if there was no change in conditions:
			// First check if any got removed(old amount equal controllers after clean-up)
			// && check if any new is about to be added(old equal new amount): [NL]
			// The reason for this is because we will get an update via the code above if there is a change in conditions. But if not we will trigger it here [NL]
			const noChangeInConditions =
				oldAmountOfControllers === this.#conditionControllers.length &&
				oldAmountOfControllers === conditionConfigs.length;
			if (noChangeInConditions) {
				if (jsonStringComparison(oldManifest, manifest) === false) {
					// There was not change in the amount of conditions, but the manifest was changed, this means this.#isPermitted is set to undefined and this will always fire the callback: [NL]
					this.#onPermissionChanged?.(this.#isPermitted, this as any);
				}
			}
		} else {
			this.#onConditionsChangedCallback();
		}
	}

	#gotConditions = (manifests: ManifestCondition[]) => {
		manifests.forEach(this.#gotCondition);
	};

	#gotCondition = async (conditionManifest: ManifestCondition) => {
		if (!this.#manifest) return;
		const conditionConfigs = this.#manifest.conditions ?? [];
		//
		// Get just the conditions that uses this condition alias:
		const configsOfThisType = conditionConfigs.filter(
			(conditionConfig) => conditionConfig.alias === conditionManifest.alias,
		);

		// Create conditions, based of condition configs:
		const newConditionControllers = await Promise.all(
			configsOfThisType.map((conditionConfig) => this.#createConditionController(conditionManifest, conditionConfig)),
		);

		// If we got destroyed in the mean time, then we don't need to continue:
		if (!this.#extensionRegistry) {
			newConditionControllers.forEach((controller) => controller?.destroy());
			return;
		}

		const oldLength = this.#conditionControllers.length;

		newConditionControllers
			.filter((x) => x !== undefined)
			.forEach((emerging) => {
				// TODO: All of this could use a refactor at one point, when someone is fresh in their mind. [NL]
				// Niels Notes: Current problem being that we are not aware about what is in the making, so we don't know if we end up creating the same condition multiple times. [NL]
				// Because it took some time to create the conditions, it maybe have already gotten created by another cycle, so lets test again. [NL]
				const existing = this.#conditionControllers.find((existing) => existing.config === emerging?.config);
				if (!existing) {
					this.#conditionControllers.push(emerging!);
				} else {
					emerging?.destroy();
				}
			});

		// If a change to amount of condition controllers, this will make sure that when new conditions are added, the callback is fired, so the extensions can be re-evaluated, starting out as bad. [NL]
		if (oldLength !== this.#conditionControllers.length) {
			this.#onConditionsChangedCallback();
		}
	};

	async #createConditionController(
		conditionManifest: ManifestCondition,
		conditionConfig: UmbConditionConfigBase,
	): Promise<UmbExtensionCondition | undefined> {
		// Check if we already have a controller for this config:
		const existing = this.#conditionControllers.find((controller) => controller.config === conditionConfig);
		if (!existing) {
			// TODO: Be aware that we might not have a host element any longer at this moment, but I did not want to make a fix for it jet, as its a good indication to if something else is terrible wrong [NL]
			const conditionController = await createExtensionApi(this, conditionManifest, [
				{
					manifest: conditionManifest,
					config: conditionConfig,
					onChange: this.#onConditionsChangedCallback,
				},
			]);
			if (conditionController) {
				return conditionController;
			}
		}
		return undefined;
	}

	#conditionsAreInitialized() {
		// Not good if we don't have a manifest.
		// Only good if conditions of manifest is equal to the amount of condition controllers (one for each condition). [NL]
		return (
			this.#manifest !== undefined && this.#conditionControllers.length === (this.#manifest.conditions ?? []).length
		);
	}

	#onConditionsChangedCallback = async () => {
		if (this.#manifest === undefined) {
			// This is cause by this controller begin destroyed in the mean time. [NL]
			// When writing this the only plausible case is a call from the conditionController to the onChange callback.
			return;
		}
		// We will collect old value here, but we need to re-collect it after a async method have been called, as it could have changed in the mean time. [NL]
		let oldValue = this.#isPermitted ?? false;

		// Find a condition that is not permitted (Notice how no conditions, means that this extension is permitted)
		const isPositive =
			this.#conditionsAreInitialized() &&
			this.#conditionControllers.some((condition) => condition.permitted === false) === false;

		this._isConditionsPositive = isPositive;

		if (isPositive === true) {
			if (this.#isPermitted !== true) {
				const newPermission = await this._conditionsAreGood();
				// Only set new permission if we are still positive, otherwise it means that we have been destroyed in the mean time. [NL]
				if (newPermission === false || this._isConditionsPositive === false) {
					// Then we need to revert the above work:
					this._conditionsAreBad();
					return;
				}
				// We update the oldValue as this point, cause in this way we are sure its the value at this point, when doing async code someone else might have changed the state in the mean time. [NL]
				oldValue = this.#isPermitted ?? false;
				this.#isPermitted = newPermission;
			}
		} else if (this.#isPermitted !== false) {
			// Clean up:
			await this._conditionsAreBad();

			// Only continue if we are still negative, otherwise it means that something changed in the mean time. [NL]
			if (this._isConditionsPositive === true) {
				return;
			}
			// We update the oldValue as this point, cause in this way we are sure its the value at this point, when doing async code someone else might have changed the state in the mean time. [NL]
			oldValue = this.#isPermitted ?? false;
			this.#isPermitted = false;
		}
		if (oldValue !== this.#isPermitted && this.#isPermitted !== undefined) {
			if (this.#isPermitted === true) {
				this.#promiseResolvers.forEach((x) => x());
				this.#promiseResolvers = [];
			}
			this.#onPermissionChanged?.(this.#isPermitted, this as any);
		}
	};

	protected abstract _conditionsAreGood(): Promise<boolean>;

	protected abstract _conditionsAreBad(): Promise<void>;

	public equal(otherClass: UmbBaseExtensionInitializer | undefined): boolean {
		return otherClass?.manifest === this.#manifest;
	}

	/*
	public hostConnected(): void {
		super.hostConnected();
		// Should not be nesecary as conditions would be reactive to connectedCallback, as they will use consumeContext. [NL]
		//this.#onConditionsChangedCallback();
	}
	*/

	/*
	public override hostDisconnected(): void {
		super.hostDisconnected();
		this._isConditionsPositive = false;
		if (this.#isPermitted === true) {
			this._conditionsAreBad();
			this.#isPermitted = false;
			this.#onPermissionChanged?.(false, this as unknown as SubClassType);
		}
	}
	*/

	#clearPermittedState() {
		if (this.#isPermitted === true) {
			this.#isPermitted = undefined;
			this._conditionsAreBad();
			this.#onPermissionChanged?.(false, this as unknown as SubClassType);
		}
	}

	public override destroy(): void {
		if (!this.#extensionRegistry) return;
		this.#manifest = undefined;
		this.#promiseResolvers = [];
		this.#clearPermittedState(); // This fires the callback as not permitted, if it was permitted before. [NL]
		this.#isPermitted = undefined;
		this._isConditionsPositive = false;
		this.#overwrites = [];
		this.#cleanConditions();
		this.#onPermissionChanged = undefined;
		(this.#extensionRegistry as unknown) = undefined;
		super.destroy();
	}
}
