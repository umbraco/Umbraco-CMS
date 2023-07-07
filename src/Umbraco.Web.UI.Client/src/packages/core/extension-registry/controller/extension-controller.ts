import type { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import { UmbBaseController, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	ManifestWithDynamicConditions,
	createExtensionClass,
} from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbExtensionController extends UmbBaseController {
	#alias: string;
	#manifest?: ManifestWithDynamicConditions;
	#conditionManifests: Array<ManifestCondition> = [];
	#conditionControllers: Array<UmbExtensionCondition> = [];
	#onPermissionChanged: () => void;
	#isPermitted = false;

	get weight() {
		return this.#manifest?.weight ?? 0;
	}

	get permitted() {
		return this.#isPermitted;
	}

	public component?: HTMLElement | null;

	constructor(host: UmbControllerHost, alias: string, onPermissionChanged: () => void) {
		super(host, alias);
		this.#alias = alias;
		this.#onPermissionChanged = onPermissionChanged;

		this.observe(umbExtensionsRegistry?.getByAlias(alias), async (extensionManifest) => {
			this.#manifest = extensionManifest as ManifestWithDynamicConditions;
			if (extensionManifest) {
				this.#gotManifest(extensionManifest as ManifestWithDynamicConditions);
			} else {
				this.removeControllerByAlias('_observeConditions');
				// TODO: more proper clean up.
			}
		});
	}

	#gotManifest(extensionManifest: ManifestWithDynamicConditions) {
		const conditionAliases = (extensionManifest.conditions ?? [])
			.map((condition) => condition.alias)
			.filter((value, index, array) => array.indexOf(value) === index);

		// Observes the conditions and initialize as they come in.
		this.observe(
			umbExtensionsRegistry?.getByTypeAndAliases('condition', conditionAliases),
			async (manifests) => {
				//const oldValue = this.#conditionManifests;
				//const oldLength = this.#conditionManifests.length;

				// Keepers:
				this.#conditionManifests = this.#conditionManifests.filter((current) =>
					manifests.find((incoming) => incoming.alias === current.alias)
				);

				// Clean up conditions controllers based on keepers:
				this.#conditionControllers = this.#conditionControllers.filter((current) => {
					const continueExistence = this.#conditionManifests.find(
						(incoming) => incoming.alias === current.controllerAlias
					);
					if (!continueExistence) {
						// Destroy condition that is no longer needed.
						current.destroy();
					}
					return continueExistence;
				});

				if (manifests.length === 0) {
					this.#onConditionsChanged();
				}
				// New comers:
				manifests.forEach(async (conditionManifest) => {
					const existing = this.#conditionManifests.find((x) => x.alias === conditionManifest.alias);
					if (!existing) {
						this.#conditionManifests.push(conditionManifest);

						// Spin up conditions, based of conditions:
						const conditionsOfThisType = extensionManifest.conditions.filter(
							(condition) => condition.alias === conditionManifest.alias
						);
						conditionsOfThisType.forEach(async (condition) => {
							const conditionController = await createExtensionClass<UmbExtensionCondition>(conditionManifest, [
								this,
								condition.value,
							]);
							if (conditionController) {
								// Some how listen to it? callback/event/onChange something.
								// then call this one: this.#onConditionsChanged();
								this.#conditionControllers.push(conditionController);
							}
						});
					}
				});
			},
			'_observeConditions'
		);

		// Initialize the conditions controllers:
		// We can get the element of the host?

		//When good then someone, somehow needs to get notified?
	}

	#conditionsAreInitialized() {
		// Not good if we don't have a manifest.
		// Only good if conditions of manifest is equal to the amount of condition controllers (one for each condition).
		return (
			this.#manifest !== undefined && this.#conditionControllers.length === (this.#manifest.conditions ?? []).length
		);
	}

	async #onConditionsChanged() {
		// Find a condition that is not permitted (Notice how no conditions, means that this extension is permitted)
		this.#isPermitted =
			this.#conditionsAreInitialized() &&
			this.#conditionControllers.find((condition) => condition.permitted === false) === undefined;

		if (this.#isPermitted) {
			await this._conditionsAreGood();
		} else {
			await this._conditionsAreBad();
		}

		this.#onPermissionChanged();
	}

	protected abstract _conditionsAreGood(): Promise<void>;

	protected abstract _conditionsAreBad(): Promise<void>;

	public destroy(): void {
		super.destroy();
		// Destroy the conditions controllers, are begin destroyed cause they are controllers.
	}
}
