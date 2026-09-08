import { UmbDeleteEntityWorkspaceRedirectController } from './delete-entity-workspace-redirect.controller.js';
import { UmbEntityDeletedEvent } from '@umbraco-cms/backoffice/entity-action';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-delete-redirect-controller-host')
class UmbTestDeleteRedirectControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/** A minimal stand-in for the workspace context — just what the controller reads. */
class UmbTestNavigationParentItemPathWorkspaceContext {
	#unique: string | null;
	#entityType: string;
	#navigationParentItemPath = new UmbStringState<string | undefined>(undefined);
	readonly navigationParentItemPath = this.#navigationParentItemPath.asObservable();

	constructor(unique: string | null, entityType: string) {
		this.#unique = unique;
		this.#entityType = entityType;
	}

	getUnique() {
		return this.#unique;
	}

	getEntityType() {
		return this.#entityType;
	}

	setNavigationParentItemPath(path: string | undefined) {
		this.#navigationParentItemPath.setValue(path);
	}
}

/**
 * Monkey-patches `window.history.pushState`/`replaceState` to record calls instead of navigating — this project
 * does not use sinon, so history calls are asserted against plain recorded-call arrays. Call `restore()` in `afterEach`.
 */
function stubHistory() {
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

describe('UmbDeleteEntityWorkspaceRedirectController', () => {
	let host: UmbTestDeleteRedirectControllerHostElement;
	let actionEventContext: UmbActionEventContext;
	let workspaceContext: UmbTestNavigationParentItemPathWorkspaceContext;
	let history: ReturnType<typeof stubHistory>;

	function createController() {
		return new UmbDeleteEntityWorkspaceRedirectController(host, workspaceContext);
	}

	function dispatchDeleted(overrides?: Partial<UmbEntityModel>) {
		actionEventContext.dispatchEvent(
			new UmbEntityDeletedEvent({ unique: 'test-unique', entityType: 'test-entity-type', ...overrides }),
		);
	}

	beforeEach(async () => {
		history = stubHistory();

		host = new UmbTestDeleteRedirectControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		workspaceContext = new UmbTestNavigationParentItemPathWorkspaceContext('test-unique', 'test-entity-type');

		await aTimeout(0);
	});

	afterEach(() => {
		history.restore();
		document.body.removeChild(host);
	});

	it('redirects to the path from navigationParentItemPath', async () => {
		workspaceContext.setNavigationParentItemPath('/test/edit/parent-unique');
		createController();
		await aTimeout(0);

		dispatchDeleted();

		expect(history.replaceStateCalls).to.have.lengthOf(1);
		expect(history.replaceStateCalls[0].url).to.equal('/test/edit/parent-unique');
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('does not redirect when navigationParentItemPath has no value', async () => {
		createController();
		await aTimeout(0);

		dispatchDeleted();

		expect(history.replaceStateCalls).to.have.lengthOf(0);
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('reacts to navigationParentItemPath updating after the controller is created', async () => {
		createController();
		await aTimeout(0);

		workspaceContext.setNavigationParentItemPath('/test/edit/parent-unique');
		await aTimeout(0);

		dispatchDeleted();

		expect(history.replaceStateCalls).to.have.lengthOf(1);
		expect(history.replaceStateCalls[0].url).to.equal('/test/edit/parent-unique');
	});

	it('does not redirect when the deleted unique does not match the open entity', async () => {
		workspaceContext.setNavigationParentItemPath('/test/root');
		createController();
		await aTimeout(0);

		dispatchDeleted({ unique: 'some-other-unique' });

		expect(history.replaceStateCalls).to.have.lengthOf(0);
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('does not redirect when the deleted entity type does not match the open entity', async () => {
		workspaceContext.setNavigationParentItemPath('/test/root');
		createController();
		await aTimeout(0);

		dispatchDeleted({ entityType: 'some-other-entity-type' });

		expect(history.replaceStateCalls).to.have.lengthOf(0);
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('destroys itself after redirecting, so a repeat event has no further effect', async () => {
		workspaceContext.setNavigationParentItemPath('/test/edit/parent-unique');
		createController();
		await aTimeout(0);

		dispatchDeleted();
		dispatchDeleted();

		expect(history.replaceStateCalls).to.have.lengthOf(1);
	});

	it('stops reacting once destroyed', async () => {
		workspaceContext.setNavigationParentItemPath('/test/root');
		const controller = createController();
		await aTimeout(0);
		controller.destroy();

		dispatchDeleted();

		expect(history.replaceStateCalls).to.have.lengthOf(0);
	});
});
