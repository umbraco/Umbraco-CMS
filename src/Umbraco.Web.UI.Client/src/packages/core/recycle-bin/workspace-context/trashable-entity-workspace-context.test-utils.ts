import type { UmbTrashableEntityWorkspaceContext } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';

@customElement('umb-test-recycle-bin-controller-host')
export class UmbTestRecycleBinControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

export interface UmbTestReadOnlyGuardRuleCall {
	action: 'add' | 'remove';
	unique: string;
}

/**
 * A minimal, hand-written `UmbTrashableEntityWorkspaceContext` — stands in for a real document/media workspace
 * context in isolated tests, recording the calls the recycle-bin context makes against it.
 */
export class UmbTestTrashableEntityWorkspaceContext implements UmbTrashableEntityWorkspaceContext {
	#host: UmbControllerHost;
	#unique = new UmbObjectState<string | null>('test-unique');
	#entityType = 'test-entity-type';
	#isTrashed = new UmbBooleanState(undefined);
	#isNew = new UmbBooleanState(undefined);

	readonly workspaceAlias = 'Umb.Test.Workspace';
	readonly unique = this.#unique.asObservable();
	readonly isTrashed = this.#isTrashed.asObservable();
	readonly isNew = this.#isNew.asObservable();
	modalContext: unknown;

	readonly readOnlyGuardRuleCalls: Array<UmbTestReadOnlyGuardRuleCall> = [];
	readonly readOnlyGuard = {
		addRule: (rule: { unique: string }) => this.readOnlyGuardRuleCalls.push({ action: 'add', unique: rule.unique }),
		removeRule: (unique: string) => this.readOnlyGuardRuleCalls.push({ action: 'remove', unique }),
	} as unknown as UmbReadOnlyVariantGuardManager;

	reloadCallCount = 0;
	resetUnpersistedChangesCallCount = 0;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	getUnique() {
		return this.#unique.getValue();
	}

	getEntityType() {
		return this.#entityType;
	}

	setUnique(unique: string | null) {
		this.#unique.setValue(unique);
	}

	setEntityType(entityType: string) {
		this.#entityType = entityType;
	}

	setIsTrashed(value: boolean | undefined) {
		this.#isTrashed.setValue(value);
	}

	setIsNew(value: boolean | undefined) {
		this.#isNew.setValue(value);
	}

	resetUnpersistedChanges(): void {
		this.resetUnpersistedChangesCallCount++;
	}

	async reload() {
		this.reloadCallCount++;
	}

	destroy() {}
}

/**
 * Monkey-patches `window.history.pushState`/`replaceState` to record calls instead of navigating — this project
 * does not use sinon, so history calls are asserted against plain recorded-call arrays. Call `restore()` in
 * `afterEach`.
 * @returns {object} The recorded call arrays, and a `restore()` function to undo the monkey-patch.
 */
export function stubHistory() {
	const originalPushState = window.history.pushState;
	const originalReplaceState = window.history.replaceState;
	const pushStateCalls: Array<{ url: string }> = [];
	const replaceStateCalls: Array<{ url: string }> = [];

	window.history.pushState = (_data: unknown, _unused: string, url?: string | URL | null) => {
		pushStateCalls.push({ url: String(url) });
	};
	window.history.replaceState = (_data: unknown, _unused: string, url?: string | URL | null) => {
		replaceStateCalls.push({ url: String(url) });
	};

	return {
		pushStateCalls,
		replaceStateCalls,
		restore: () => {
			window.history.pushState = originalPushState;
			window.history.replaceState = originalReplaceState;
		},
	};
}
