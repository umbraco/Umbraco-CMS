import { UmbDeleteEntityWorkspaceRedirectController } from './delete-entity-workspace-redirect.controller.js';
import { UmbEntityDeletedEvent } from '@umbraco-cms/backoffice/entity-action';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbParentEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-test-delete-redirect-controller-host')
class UmbTestDeleteRedirectControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/** A minimal `UmbEntityWorkspaceContext` stand-in — just the identity the controller reads. */
class UmbTestEntityWorkspaceContext implements Pick<UmbEntityWorkspaceContext, 'getUnique' | 'getEntityType'> {
	#unique: string | null;
	#entityType: string;

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
	let parentEntityContext: UmbParentEntityContext;
	let workspaceContext: UmbTestEntityWorkspaceContext;
	let history: ReturnType<typeof stubHistory>;
	let redirectPathCalls: Array<UmbEntityModel | undefined>;

	function createController() {
		return new UmbDeleteEntityWorkspaceRedirectController(host, workspaceContext as unknown as UmbEntityWorkspaceContext, {
			getRedirectPath: ({ entity }) => {
				redirectPathCalls.push(entity);
				return entity ? `/test/edit/${entity.unique}` : '/test/root';
			},
		});
	}

	function dispatchDeleted(overrides?: Partial<UmbEntityModel>) {
		actionEventContext.dispatchEvent(
			new UmbEntityDeletedEvent({ unique: 'test-unique', entityType: 'test-entity-type', ...overrides }),
		);
	}

	beforeEach(async () => {
		history = stubHistory();
		redirectPathCalls = [];

		host = new UmbTestDeleteRedirectControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		parentEntityContext = new UmbParentEntityContext(host);
		workspaceContext = new UmbTestEntityWorkspaceContext('test-unique', 'test-entity-type');

		await aTimeout(0);
	});

	afterEach(() => {
		history.restore();
		document.body.removeChild(host);
	});

	it('redirects to the parent from UMB_PARENT_ENTITY_CONTEXT', async () => {
		parentEntityContext.setParent({ unique: 'parent-unique', entityType: 'parent-entity-type' });
		createController();
		await aTimeout(0);

		dispatchDeleted();

		expect(redirectPathCalls).to.have.lengthOf(1);
		expect(redirectPathCalls[0]).to.deep.equal({ unique: 'parent-unique', entityType: 'parent-entity-type' });
		expect(history.replaceStateCalls).to.have.lengthOf(1);
		expect(history.replaceStateCalls[0].url).to.equal('/test/edit/parent-unique');
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('redirects to the fallback path when the deleted entity had no parent (root)', async () => {
		createController();
		await aTimeout(0);

		dispatchDeleted();

		expect(redirectPathCalls).to.have.lengthOf(1);
		expect(redirectPathCalls[0]).to.equal(undefined);
		expect(history.replaceStateCalls).to.have.lengthOf(1);
		expect(history.replaceStateCalls[0].url).to.equal('/test/root');
	});

	it('does not redirect when the deleted unique does not match the open entity', async () => {
		createController();
		await aTimeout(0);

		dispatchDeleted({ unique: 'some-other-unique' });

		expect(history.replaceStateCalls).to.have.lengthOf(0);
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('does not redirect when the deleted entity type does not match the open entity', async () => {
		createController();
		await aTimeout(0);

		dispatchDeleted({ entityType: 'some-other-entity-type' });

		expect(history.replaceStateCalls).to.have.lengthOf(0);
		expect(history.pushStateCalls).to.have.lengthOf(0);
	});

	it('destroys itself after redirecting, so a repeat event has no further effect', async () => {
		parentEntityContext.setParent({ unique: 'parent-unique', entityType: 'parent-entity-type' });
		createController();
		await aTimeout(0);

		dispatchDeleted();
		dispatchDeleted();

		expect(redirectPathCalls).to.have.lengthOf(1);
		expect(history.replaceStateCalls).to.have.lengthOf(1);
	});

	it('stops reacting once destroyed', async () => {
		const controller = createController();
		await aTimeout(0);
		controller.destroy();

		dispatchDeleted();

		expect(history.replaceStateCalls).to.have.lengthOf(0);
	});
});
