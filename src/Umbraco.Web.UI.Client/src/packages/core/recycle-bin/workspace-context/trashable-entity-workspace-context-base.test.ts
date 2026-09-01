import { UmbTrashableEntityWorkspaceContextBase } from './trashable-entity-workspace-context-base.js';
import { UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT } from './trashable-entity-workspace.context-token.js';
import {
	UmbTestRecycleBinControllerHostElement,
	UmbTestRecycleBinRepository,
	UmbTestTrashableEntityWorkspaceContext,
	createTestRecycleBinRepositoryManifest,
	stubHistory,
} from './trashable-entity-workspace-context.test-utils.js';
import { UMB_IS_TRASHED_ENTITY_CONTEXT } from '../contexts/is-trashed/constants.js';
import { UmbEntityRestoredFromRecycleBinEvent, UmbEntityTrashedEvent } from '../entity-action/index.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const TEST_REPOSITORY_ALIAS = 'Umb.Test.TrashableEntityWorkspaceContextBase.RecycleBinRepository';

class TestTrashableEntityWorkspaceContext extends UmbTrashableEntityWorkspaceContextBase {
	readonly redirectPathCalls: Array<UmbEntityModel> = [];

	constructor(host: UmbControllerHost) {
		super(host);
		this._setRecycleBinRepositoryAlias(TEST_REPOSITORY_ALIAS);
	}

	protected override getRedirectPath({ entity }: { entity: UmbEntityModel }): string {
		this.redirectPathCalls.push(entity);
		return entity.unique ? `/test/edit/${entity.unique}` : '/test/root';
	}
}

/** Configured with a repository alias that is never registered, so `#redirectToParent` always rejects. */
class TestTrashableEntityWorkspaceContextWithMissingRepository extends UmbTrashableEntityWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this._setRecycleBinRepositoryAlias('Umb.Test.TrashableEntityWorkspaceContextBase.MissingRecycleBinRepository');
	}

	protected override getRedirectPath(): string {
		return '/test/never-reached';
	}
}

describe('UmbTrashableEntityWorkspaceContextBase', () => {
	let host: UmbTestRecycleBinControllerHostElement;
	let actionEventContext: UmbActionEventContext;
	let workspaceContext: UmbTestTrashableEntityWorkspaceContext;
	let context: TestTrashableEntityWorkspaceContext;
	let history: ReturnType<typeof stubHistory>;

	before(() => {
		umbExtensionsRegistry.register(createTestRecycleBinRepositoryManifest(TEST_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestRecycleBinRepository.reset();
		history = stubHistory();

		host = new UmbTestRecycleBinControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		workspaceContext = new UmbTestTrashableEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);

		context = new TestTrashableEntityWorkspaceContext(host);

		// The initial (undefined) `isTrashed` emission calls removeRule() once as a no-op baseline — clear it so
		// each test's assertions only see calls made by that test's own actions.
		await aTimeout(0);
		workspaceContext.readOnlyGuardRuleCalls.length = 0;
	});

	afterEach(() => {
		history.restore();
		document.body.removeChild(host);
	});

	function dispatchTrashed(overrides?: Partial<UmbEntityModel>) {
		actionEventContext.dispatchEvent(
			new UmbEntityTrashedEvent({ unique: 'test-unique', entityType: 'test-entity-type', ...overrides }),
		);
	}

	function dispatchRestored(overrides?: Partial<UmbEntityModel>) {
		actionEventContext.dispatchEvent(
			new UmbEntityRestoredFromRecycleBinEvent({ unique: 'test-unique', entityType: 'test-entity-type', ...overrides }),
		);
	}

	describe('readonly guard', () => {
		it('adds a readonly rule when the workspace becomes trashed', async () => {
			workspaceContext.setIsTrashed(true);
			await aTimeout(0);

			expect(workspaceContext.readOnlyGuardRuleCalls).to.have.lengthOf(1);
			expect(workspaceContext.readOnlyGuardRuleCalls[0].action).to.equal('add');
		});

		it('removes the readonly rule when the workspace is no longer trashed', async () => {
			workspaceContext.setIsTrashed(true);
			await aTimeout(0);
			workspaceContext.setIsTrashed(false);
			await aTimeout(0);

			const lastCall = workspaceContext.readOnlyGuardRuleCalls.at(-1);
			expect(lastCall?.action).to.equal('remove');
		});

		it('marks the entity as trashed via UMB_IS_TRASHED_ENTITY_CONTEXT', async () => {
			const isTrashedContext = await context.getContext(UMB_IS_TRASHED_ENTITY_CONTEXT);

			workspaceContext.setIsTrashed(true);
			await aTimeout(0);
			expect(isTrashedContext?.getIsTrashed()).to.be.true;

			workspaceContext.setIsTrashed(false);
			await aTimeout(0);
			expect(isTrashedContext?.getIsTrashed()).to.be.false;
		});

		it('resets the trashed state when the workspace becomes new', async () => {
			const isTrashedContext = await context.getContext(UMB_IS_TRASHED_ENTITY_CONTEXT);

			workspaceContext.setIsTrashed(true);
			await aTimeout(0);
			expect(isTrashedContext?.getIsTrashed()).to.be.true;

			workspaceContext.setIsNew(true);
			await aTimeout(0);
			expect(isTrashedContext?.getIsTrashed()).to.be.false;
		});
	});

	describe('reload on trash/restore', () => {
		it('does not reload the workspace when the trashed entity matches (redirects instead)', async () => {
			dispatchTrashed();
			await aTimeout(0);

			expect(workspaceContext.reloadCallCount).to.equal(0);
		});

		it('reloads the workspace when the trashed entity matches and is hosted in a modal', async () => {
			workspaceContext.modalContext = { data: {} };

			dispatchTrashed();
			await aTimeout(0);

			expect(workspaceContext.reloadCallCount).to.equal(1);
		});

		it('reloads the workspace when the restored entity matches', async () => {
			dispatchRestored();
			await aTimeout(0);

			expect(workspaceContext.reloadCallCount).to.equal(1);
		});

		it('does not reload when the trashed unique does not match the open entity', async () => {
			dispatchTrashed({ unique: 'some-other-unique' });
			await aTimeout(0);

			expect(workspaceContext.reloadCallCount).to.equal(0);
		});

		it('does not reload when the trashed entity type does not match the open entity', async () => {
			dispatchTrashed({ entityType: 'some-other-entity-type' });
			await aTimeout(0);

			expect(workspaceContext.reloadCallCount).to.equal(0);
		});
	});

	describe('redirect on trash', () => {
		it('redirects to the parent when the trashed entity had a parent', async () => {
			UmbTestRecycleBinRepository.originalParent = { unique: 'parent-unique' };

			dispatchTrashed();
			await aTimeout(50);

			expect(UmbTestRecycleBinRepository.requestOriginalParentCalls).to.deep.equal(['test-unique']);
			expect(context.redirectPathCalls).to.have.lengthOf(1);
			expect(context.redirectPathCalls[0]).to.deep.equal({ entityType: 'test-entity-type', unique: 'parent-unique' });
			expect(history.replaceStateCalls).to.have.lengthOf(1);
			expect(history.replaceStateCalls[0].url).to.equal('/test/edit/parent-unique');
			expect(history.pushStateCalls).to.have.lengthOf(0);
			// Redirecting shouldn't also reload — reload() is for the modal (stay-put) case only.
			expect(workspaceContext.reloadCallCount).to.equal(0);
		});

		it('redirects to the fallback path when the trashed entity had no parent (root)', async () => {
			UmbTestRecycleBinRepository.originalParent = null;

			dispatchTrashed();
			await aTimeout(50);

			expect(context.redirectPathCalls).to.have.lengthOf(1);
			expect(context.redirectPathCalls[0]).to.deep.equal({ entityType: 'test-entity-type', unique: null });
			expect(history.pushStateCalls).to.have.lengthOf(1);
			expect(history.pushStateCalls[0].url).to.equal('/test/root');
			expect(history.replaceStateCalls).to.have.lengthOf(0);
			expect(workspaceContext.reloadCallCount).to.equal(0);
		});

		it('does not redirect when hosted in a modal, but still reloads', async () => {
			workspaceContext.modalContext = { data: {} };
			UmbTestRecycleBinRepository.originalParent = { unique: 'parent-unique' };

			dispatchTrashed();
			await aTimeout(50);

			expect(workspaceContext.reloadCallCount).to.equal(1);
			expect(UmbTestRecycleBinRepository.requestOriginalParentCalls).to.have.lengthOf(0);
			expect(history.pushStateCalls).to.have.lengthOf(0);
			expect(history.replaceStateCalls).to.have.lengthOf(0);
		});

		it('does not redirect for a restored entity', async () => {
			UmbTestRecycleBinRepository.originalParent = { unique: 'parent-unique' };

			dispatchRestored();
			await aTimeout(50);

			expect(UmbTestRecycleBinRepository.requestOriginalParentCalls).to.have.lengthOf(0);
			expect(history.pushStateCalls).to.have.lengthOf(0);
			expect(history.replaceStateCalls).to.have.lengthOf(0);
		});

		it('does not redirect when a different entity was trashed', async () => {
			UmbTestRecycleBinRepository.originalParent = { unique: 'parent-unique' };

			dispatchTrashed({ unique: 'some-other-unique' });
			await aTimeout(50);

			expect(UmbTestRecycleBinRepository.requestOriginalParentCalls).to.have.lengthOf(0);
			expect(history.pushStateCalls).to.have.lengthOf(0);
			expect(history.replaceStateCalls).to.have.lengthOf(0);
		});
	});

	describe('redirect failure', () => {
		it('falls back to reloading in place when the redirect rejects', async () => {
			const failHost = new UmbTestRecycleBinControllerHostElement();
			document.body.appendChild(failHost);

			const failActionEventContext = new UmbActionEventContext(failHost);
			const failWorkspaceContext = new UmbTestTrashableEntityWorkspaceContext(failHost);
			new UmbContextProviderController(failHost, UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT, failWorkspaceContext as never);
			new TestTrashableEntityWorkspaceContextWithMissingRepository(failHost);
			await aTimeout(0);

			failActionEventContext.dispatchEvent(
				new UmbEntityTrashedEvent({ unique: 'test-unique', entityType: 'test-entity-type' }),
			);
			await aTimeout(50);

			expect(failWorkspaceContext.reloadCallCount).to.equal(1);

			document.body.removeChild(failHost);
		});
	});

	describe('destroy', () => {
		it('stops reacting to trash events once destroyed', async () => {
			context.destroy();

			dispatchTrashed();
			await aTimeout(0);

			expect(workspaceContext.reloadCallCount).to.equal(0);
		});
	});
});
