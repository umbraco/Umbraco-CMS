import type { UmbPermissionTypeEvaluatorApi, UmbPermissionTypeEvaluatorVerdict } from '../evaluator/types.js';
import type { UmbPermission } from '../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-permission-host')
export class UmbTestPermissionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * Creates a fake evaluator class whose verdicts the test controls.
 * @returns {typeof UmbControllerBase} The class, which records the instances it creates.
 */
export function createTestEvaluatorClass() {
	return class UmbTestPermissionTypeEvaluator extends UmbControllerBase implements UmbPermissionTypeEvaluatorApi {
		static instances: Array<UmbTestPermissionTypeEvaluator> = [];

		receivedPermissions: Array<Array<UmbPermission>> = [];
		permissionsWhenAsked?: Array<UmbPermission>;
		askedVerbs?: Array<string>;
		destroyed = false;
		verdictsState = new UmbBasicState<Record<string, UmbPermissionTypeEvaluatorVerdict> | undefined>(undefined);

		constructor(host: UmbControllerHost) {
			super(host);
			UmbTestPermissionTypeEvaluator.instances.push(this);
		}

		setPermissions(permissions: Array<UmbPermission>) {
			this.receivedPermissions.push(permissions);
		}

		verdicts(verbs: Array<string>) {
			this.askedVerbs = verbs;
			this.permissionsWhenAsked = this.receivedPermissions.at(-1);
			return this.verdictsState.asObservable();
		}

		override destroy() {
			this.destroyed = true;
			super.destroy();
		}
	};
}

/**
 * Waits until the check returns true.
 * @param {() => boolean} check - The condition to wait for.
 */
export async function waitFor(check: () => boolean) {
	for (let attempt = 0; attempt < 50; attempt++) {
		if (check()) return;
		await new Promise((resolve) => setTimeout(resolve, 10));
	}
	throw new Error('Timed out waiting for condition');
}

/**
 * Runs the callback with console.error silenced, for tests of code that logs an expected error.
 * @param {() => Promise<void>} callback - The test code.
 */
export async function withoutConsoleError(callback: () => Promise<void>) {
	const originalError = console.error;
	console.error = () => {};
	try {
		await callback();
	} finally {
		console.error = originalError;
	}
}
