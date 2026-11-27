import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import type { ManifestPermissionTypeEvaluator } from '../evaluator/permission-type-evaluator.extension.js';
import type { UmbPermissionTypeEvaluatorVerdict } from '../evaluator/types.js';
import type { UmbPermission, UmbPermissionConfig, UmbPermissionDenyReason, UmbPermissionResult } from '../types.js';
import type { UmbPermissionRegistry } from './permission.registry.js';
import { UmbPermissionTypeEvaluatorController } from './permission-type-evaluator.controller.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { createObservablePart, mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

interface UmbPermissionVerbResult {
	permitted: boolean | undefined;
	denyReasons: Array<UmbPermissionDenyReason>;
}

interface UmbPermissionEvaluatorVerdicts {
	manifest: ManifestPermissionTypeEvaluator;
	/** `undefined` until the evaluator has answered. */
	verdicts: Record<string, UmbPermissionTypeEvaluatorVerdict> | undefined;
}

/**
 * Checks the verbs of one config where they are asked. Keeps an evaluator for each evaluator manifest that lists a
 * requested verb, and decides the result from the global permissions and the evaluators' verdicts.
 */
export class UmbPermissionCheckController extends UmbControllerBase {
	#registry: UmbPermissionRegistry;
	#verbs: Array<string>;
	#evaluators = new Map<string, UmbPermissionTypeEvaluatorController>();
	#evaluatorVerdicts = new UmbArrayState<UmbPermissionEvaluatorVerdicts>([], (entry) => entry.manifest.alias);

	readonly result: Observable<UmbPermissionResult>;
	readonly #determinedResult: Observable<UmbPermissionResult | undefined>;

	constructor(host: UmbControllerHost, registry: UmbPermissionRegistry, config: UmbPermissionConfig) {
		super(host);
		this.#registry = registry;
		this.#verbs = [...new Set([...(config.allOf ?? []), ...(config.oneOf ?? [])])];

		this.observe(
			umbExtensionsRegistry.byTypeAndFilter('permissionTypeEvaluator', (manifest) =>
				manifest.forPermissionVerbs.some((verb) => this.#verbs.includes(verb)),
			),
			(manifests) => this.#gotManifests(manifests),
			null,
		);

		// TODO: Needs #24455 to be merged first. Until then the derived observables below stay subscribed to the
		// registry's sources after this check is destroyed, which keeps the check and its host in memory.
		this.result = mergeObservables(
			[registry.permissionsOfType(UMB_GLOBAL_PERMISSION_TYPE), this.#evaluatorVerdicts.asObservable()],
			([globalPermissions, evaluators]) =>
				this.#combineResults(config, (verb) => this.#verbResult(verb, globalPermissions, evaluators)),
		);
		this.#determinedResult = createObservablePart(this.result, (result) =>
			result.permitted === undefined ? undefined : result,
		);
	}

	/**
	 * The first result where `permitted` isn't `undefined`.
	 * @returns {Promise<UmbPermissionResult>} The result.
	 */
	getDeterminedResult(): Promise<UmbPermissionResult> {
		return this.observe(this.#determinedResult, undefined, null).asPromise();
	}

	#gotManifests(manifests: Array<ManifestPermissionTypeEvaluator>) {
		this.#evaluators.forEach((evaluator, alias) => {
			if (manifests.some((manifest) => manifest.alias === alias)) return;
			this.removeUmbControllerByAlias(this.#permissionsObserverAlias(alias));
			this.removeUmbControllerByAlias(this.#verdictsObserverAlias(alias));
			evaluator.destroy();
			this.#evaluators.delete(alias);
			this.#evaluatorVerdicts.removeOne(alias);
		});

		manifests.forEach((manifest) => {
			if (this.#evaluators.has(manifest.alias)) return;
			const verbs = this.#verbs.filter((verb) => manifest.forPermissionVerbs.includes(verb));
			const evaluator = new UmbPermissionTypeEvaluatorController(this, manifest, verbs);
			this.#evaluators.set(manifest.alias, evaluator);
			this.observe(
				this.#registry.permissionsOfType(manifest.forPermissionType),
				(permissions) => evaluator.setPermissions(permissions),
				this.#permissionsObserverAlias(manifest.alias),
			);
			this.observe(
				evaluator.verdicts,
				(verdicts) => this.#evaluatorVerdicts.appendOne({ manifest, verdicts }),
				this.#verdictsObserverAlias(manifest.alias),
			);
		});
	}

	#permissionsObserverAlias(alias: string) {
		return `umbPermissionsObserver_${alias}`;
	}

	#verdictsObserverAlias(alias: string) {
		return `umbVerdictsObserver_${alias}`;
	}

	/**
	 * Decides one verb: a global deny blocks it; otherwise the verdicts of the evaluators that list it decide, and
	 * a global allow decides when none of them has a verdict. A denying verdict decides before pending evaluators
	 * have answered.
	 * @param {string} verb - The verb to decide.
	 * @param {Array<UmbPermission>} globalPermissions - The permissions of the global type.
	 * @param {Array<UmbPermissionEvaluatorVerdicts>} evaluators - The verdicts of the evaluators.
	 * @returns {UmbPermissionVerbResult} The result for the verb.
	 */
	#verbResult(
		verb: string,
		globalPermissions: Array<UmbPermission>,
		evaluators: Array<UmbPermissionEvaluatorVerdicts>,
	): UmbPermissionVerbResult {
		const globalDenies = globalPermissions.filter((permission) => permission.deny?.includes(verb));
		if (globalDenies.length > 0) {
			return {
				permitted: false,
				denyReasons: globalDenies.flatMap((permission) => (permission.denyReason ? [permission.denyReason] : [])),
			};
		}

		const listing = evaluators.filter((evaluator) => evaluator.manifest.forPermissionVerbs.includes(verb));
		const verdicts = listing.flatMap((evaluator) => {
			const verdict = evaluator.verdicts?.[verb];
			return verdict ? [verdict] : [];
		});

		const denyingVerdicts = verdicts.filter((verdict) => !verdict.permitted);
		if (denyingVerdicts.length > 0) {
			return { permitted: false, denyReasons: denyingVerdicts.flatMap((verdict) => verdict.denyReasons ?? []) };
		}

		if (listing.some((evaluator) => evaluator.verdicts === undefined)) {
			return { permitted: undefined, denyReasons: [] };
		}

		if (verdicts.length > 0) {
			return { permitted: true, denyReasons: [] };
		}

		const globallyAllowed = globalPermissions.some((permission) => permission.allow?.includes(verb));
		return { permitted: globallyAllowed, denyReasons: [] };
	}

	/**
	 * Combines the results of the configured verbs. A denied verb decides the result before undetermined
	 * ones are known. An empty config is denied.
	 * @param {UmbPermissionConfig} config - The verbs to check.
	 * @param {(verb: string) => UmbPermissionVerbResult} getVerbResult - The result of a single verb.
	 * @returns {UmbPermissionResult} The combined result.
	 */
	#combineResults(
		config: UmbPermissionConfig,
		getVerbResult: (verb: string) => UmbPermissionVerbResult,
	): UmbPermissionResult {
		const allOf = (config.allOf ?? []).map(getVerbResult);
		const oneOf = (config.oneOf ?? []).map(getVerbResult);

		if (allOf.length === 0 && oneOf.length === 0) {
			return { permitted: false, denyReasons: [] };
		}

		const allOfPermitted = this.#allPermitted(allOf.map((result) => result.permitted));
		const oneOfPermitted = oneOf.length > 0 ? this.#anyPermitted(oneOf.map((result) => result.permitted)) : true;
		const permitted = this.#allPermitted([allOfPermitted, oneOfPermitted]);

		if (permitted !== false) {
			return { permitted, denyReasons: [] };
		}

		const denied = [...allOf, ...oneOf].filter((result) => result.permitted === false);
		return { permitted, denyReasons: this.#uniqueDenyReasons(denied.flatMap((result) => result.denyReasons)) };
	}

	/**
	 * Three-valued AND: false wins, then undefined.
	 * @param {Array<boolean | undefined>} values - The values to combine.
	 * @returns {boolean | undefined} The combined value.
	 */
	#allPermitted(values: Array<boolean | undefined>): boolean | undefined {
		if (values.includes(false)) return false;
		if (values.includes(undefined)) return undefined;
		return true;
	}

	/**
	 * Three-valued OR: true wins, then undefined.
	 * @param {Array<boolean | undefined>} values - The values to combine.
	 * @returns {boolean | undefined} The combined value.
	 */
	#anyPermitted(values: Array<boolean | undefined>): boolean | undefined {
		if (values.includes(true)) return true;
		if (values.includes(undefined)) return undefined;
		return false;
	}

	/**
	 * Removes duplicate deny reasons.
	 * @param {Array<UmbPermissionDenyReason>} reasons - The reasons.
	 * @returns {Array<UmbPermissionDenyReason>} The reasons, each once.
	 */
	#uniqueDenyReasons(reasons: Array<UmbPermissionDenyReason>): Array<UmbPermissionDenyReason> {
		const seen = new Set<string>();
		return reasons.filter((reason) => {
			const key = JSON.stringify(reason);
			if (seen.has(key)) return false;
			seen.add(key);
			return true;
		});
	}

	override destroy(): void {
		super.destroy();
		this.#evaluators.clear();
		this.#evaluatorVerdicts.destroy();
	}
}
