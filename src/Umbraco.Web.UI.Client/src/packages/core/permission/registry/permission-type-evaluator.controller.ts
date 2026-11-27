import type { ManifestPermissionTypeEvaluator } from '../evaluator/permission-type-evaluator.extension.js';
import type { UmbPermissionTypeEvaluatorVerdict } from '../evaluator/types.js';
import type { UmbPermission } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Creates one permission type evaluator, passes it the permissions it is given, and holds its verdicts.
 * An evaluator that can't be created denies the verbs.
 */
export class UmbPermissionTypeEvaluatorController extends UmbControllerBase {
	#permissions = new UmbBasicState<Array<UmbPermission>>([]);

	#verdicts = new UmbBasicState<Record<string, UmbPermissionTypeEvaluatorVerdict> | undefined>(undefined);
	/** `undefined` until the evaluator has answered. */
	readonly verdicts = this.#verdicts.asObservable();

	#destroyed = false;

	constructor(host: UmbControllerHost, manifest: ManifestPermissionTypeEvaluator, verbs: Array<string>) {
		super(host);
		this.#create(manifest, verbs);
	}

	/**
	 * Sets the permissions of the evaluator's type.
	 * @param {Array<UmbPermission>} permissions - The permissions.
	 */
	setPermissions(permissions: Array<UmbPermission>): void {
		this.#permissions.setValue(permissions);
	}

	async #create(manifest: ManifestPermissionTypeEvaluator, verbs: Array<string>) {
		const api = await createExtensionApi(this, manifest).catch((error) => {
			console.error(`Permission type evaluator "${manifest.alias}" could not be created.`, error);
			return undefined;
		});

		if (this.#destroyed) {
			api?.destroy?.();
			return;
		}

		if (!api) {
			this.#verdicts.setValue(Object.fromEntries(verbs.map((verb) => [verb, { permitted: false }])));
			return;
		}

		this.observe(this.#permissions.asObservable(), (permissions) => api.setPermissions(permissions), null);
		this.observe(api.verdicts(verbs), (verdicts) => this.#verdicts.setValue(verdicts), null);
	}

	override destroy(): void {
		this.#destroyed = true;
		super.destroy();
		this.#permissions.destroy();
		this.#verdicts.destroy();
	}
}
