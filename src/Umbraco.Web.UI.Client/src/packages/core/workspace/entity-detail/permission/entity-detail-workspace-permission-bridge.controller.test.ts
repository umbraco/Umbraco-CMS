import { UmbEntityDetailWorkspaceContextBase } from '../entity-detail-workspace-base.js';
import {
	UmbEntityDetailWorkspacePermissionBridgeController,
	type UmbEntityDetailWorkspacePermissionBridgeControllerArgs,
} from './entity-detail-workspace-permission-bridge.controller.js';
import { createTestEvaluatorClass, waitFor } from '../../../permission/registry/permission.test-utils.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_GLOBAL_PERMISSION_TYPE,
	umbPermissionRegistry,
	type UmbPermission,
	type UmbPermissionDenyReason,
} from '@umbraco-cms/backoffice/permission';

const CREATE = 'Test.EntityDetailWorkspace.Create';
const UPDATE = 'Test.EntityDetailWorkspace.Update';
const SOURCE = 'Test.EntityDetailWorkspacePermission';
const EVALUATOR_ALIAS = 'Test.EntityDetailWorkspacePermission.Evaluator';
const UNIQUE = 'umbEntityDetailWorkspacePermission';
const LOCKED: UmbPermissionDenyReason = { title: 'Locked', detail: 'Unlock it first' };
const ARCHIVED: UmbPermissionDenyReason = { title: 'Archived', detail: 'Restore it first' };

@customElement('umb-test-entity-detail-workspace-permission-host')
class UmbTestEntityDetailWorkspacePermissionHostElement extends UmbElementMixin(HTMLElement) {}

class UmbTestEntityDetailWorkspaceContext extends UmbEntityDetailWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: 'Umb.Test.Workspace.EntityDetailPermission',
			entityType: 'umb-test-entity-detail-permission',
			detailRepositoryAlias: 'Umb.Test.Repository.EntityDetailPermission',
		});
	}

	setNew(isNew: boolean) {
		this.setIsNew(isNew);
	}
}

function allow(...verbs: Array<string>): UmbPermission {
	return { type: UMB_GLOBAL_PERMISSION_TYPE, allow: verbs };
}

function deny(verb: string, denyReason?: UmbPermissionDenyReason): UmbPermission {
	return { type: UMB_GLOBAL_PERMISSION_TYPE, deny: [verb], denyReason };
}

describe('UmbEntityDetailWorkspacePermissionBridgeController', () => {
	let host: UmbTestEntityDetailWorkspacePermissionHostElement;
	let context: UmbTestEntityDetailWorkspaceContext;

	beforeEach(() => {
		host = new UmbTestEntityDetailWorkspacePermissionHostElement();
		document.body.appendChild(host);
		context = new UmbTestEntityDetailWorkspaceContext(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
		umbPermissionRegistry.removeSource(SOURCE);
		umbExtensionsRegistry.unregister(EVALUATOR_ALIAS);
	});

	async function createController(
		args: UmbEntityDetailWorkspacePermissionBridgeControllerArgs = { create: CREATE, update: UPDATE },
	) {
		const controller = new UmbEntityDetailWorkspacePermissionBridgeController(context, args);
		await aTimeout(0);
		return controller;
	}

	function permissionRule() {
		return context.readOnlyGuard.getRules().find((rule) => rule.unique === UNIQUE);
	}

	function forbiddenState() {
		return context.forbidden.getStates().find((state) => state.unique === UNIQUE);
	}

	function registerPendingEvaluator(verbs: Array<string>) {
		const api = createTestEvaluatorClass();
		umbExtensionsRegistry.register({
			type: 'permissionTypeEvaluator',
			alias: EVALUATOR_ALIAS,
			name: EVALUATOR_ALIAS,
			forPermissionType: 'Test.EntityDetailWorkspacePermission.Type',
			forPermissionVerbs: verbs,
			api,
		});
		return api;
	}

	describe('an existing entity', () => {
		beforeEach(() => context.setNew(false));

		it('is read-only while update is denied, with the first deny reason as message', async () => {
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE, UPDATE), deny(UPDATE, LOCKED)]);
			await createController();

			expect(permissionRule()).to.deep.equal({ unique: UNIQUE, permitted: true, message: LOCKED.title });
			expect(context.readOnlyGuard.getPermitted()).to.be.true;
		});

		it('is not read-only while update is permitted', async () => {
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE, UPDATE)]);
			await createController();

			expect(permissionRule()).to.be.undefined;
			expect(context.readOnlyGuard.getPermitted()).to.be.false;
		});

		it('is neither read-only nor forbidden when create is denied', async () => {
			umbPermissionRegistry.setSource(SOURCE, [allow(UPDATE), deny(CREATE, LOCKED)]);
			await createController();

			expect(permissionRule()).to.be.undefined;
			expect(forbiddenState()).to.be.undefined;
		});

		it('is not forbidden when update is denied', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(UPDATE, LOCKED)]);
			await createController();

			expect(forbiddenState()).to.be.undefined;
		});

		it('is not read-only while update is undetermined, and becomes read-only once it is denied', async () => {
			const evaluator = registerPendingEvaluator([UPDATE]);
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE, UPDATE)]);
			await createController();

			expect(permissionRule()).to.be.undefined;

			await waitFor(() => evaluator.instances.length > 0);
			evaluator.instances[0].verdictsState.setValue({ [UPDATE]: { permitted: false, denyReasons: [LOCKED] } });

			expect(permissionRule()?.message).to.equal(LOCKED.title);
		});

		it('is no longer read-only once update is permitted again', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(UPDATE, LOCKED)]);
			await createController();
			umbPermissionRegistry.setSource(SOURCE, [allow(UPDATE)]);

			expect(permissionRule()).to.be.undefined;
		});

		it('updates the message when the deny reason changes', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(UPDATE, LOCKED)]);
			await createController();
			umbPermissionRegistry.setSource(SOURCE, [deny(UPDATE, ARCHIVED)]);

			expect(permissionRule()?.message).to.equal(ARCHIVED.title);
		});

		it('is read-only without a message when update is denied without a reason', async () => {
			await createController();

			expect(permissionRule()).to.deep.equal({ unique: UNIQUE, permitted: true, message: undefined });
		});

		it('is not read-only without an update verb', async () => {
			await createController({ create: CREATE });

			expect(permissionRule()).to.be.undefined;
		});
	});

	describe('a new entity', () => {
		beforeEach(() => context.setNew(true));

		it('is forbidden and read-only while create is denied, with the first deny reason', async () => {
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE, UPDATE), deny(CREATE, LOCKED)]);
			await createController();

			expect(forbiddenState()).to.deep.equal({ unique: UNIQUE, reason: LOCKED });
			expect(permissionRule()?.message).to.equal(LOCKED.title);
		});

		it('is forbidden without a reason when create is denied without one', async () => {
			await createController();

			expect(forbiddenState()).to.deep.equal({ unique: UNIQUE, reason: undefined });
		});

		it('is neither forbidden nor read-only while create is permitted', async () => {
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE, UPDATE)]);
			await createController();

			expect(forbiddenState()).to.be.undefined;
			expect(permissionRule()).to.be.undefined;
		});

		it('is neither forbidden nor read-only when only update is denied', async () => {
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE), deny(UPDATE, LOCKED)]);
			await createController();

			expect(forbiddenState()).to.be.undefined;
			expect(permissionRule()).to.be.undefined;
		});

		it('is not forbidden while create is undetermined, and becomes forbidden once it is denied', async () => {
			const evaluator = registerPendingEvaluator([CREATE]);
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE, UPDATE)]);
			await createController();

			expect(forbiddenState()).to.be.undefined;
			expect(permissionRule()).to.be.undefined;

			await waitFor(() => evaluator.instances.length > 0);
			evaluator.instances[0].verdictsState.setValue({ [CREATE]: { permitted: false, denyReasons: [LOCKED] } });

			expect(forbiddenState()?.reason).to.deep.equal(LOCKED);
		});

		it('is no longer forbidden once create is permitted again', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, LOCKED)]);
			await createController();
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE)]);

			expect(forbiddenState()).to.be.undefined;
			expect(permissionRule()).to.be.undefined;
		});

		it('updates the forbidden state when the deny reason changes', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, LOCKED)]);
			await createController();
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, ARCHIVED)]);

			expect(forbiddenState()).to.deep.equal({ unique: UNIQUE, reason: ARCHIVED });
		});

		it('keeps the forbidden state when it applies again with the same reason', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, LOCKED)]);
			await createController();
			const states: Array<unknown> = [];
			const subscription = context.forbidden.states.subscribe((value) => states.push(value));
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, { ...LOCKED }), allow(UPDATE)]);
			subscription.unsubscribe();

			expect(states).to.have.length(1);
		});

		it('updates the forbidden state when only the detail of the reason changes', async () => {
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, LOCKED)]);
			await createController();
			umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, { title: LOCKED.title, detail: 'Ask an administrator' })]);

			expect(forbiddenState()?.reason).to.deep.equal({ title: LOCKED.title, detail: 'Ask an administrator' });
		});

		it('is neither forbidden nor read-only without a create verb', async () => {
			await createController({ update: UPDATE });

			expect(forbiddenState()).to.be.undefined;
			expect(permissionRule()).to.be.undefined;
		});

		it('keeps other forbidden states', async () => {
			context.forbidden.addState({ unique: 'other' });
			await createController();
			umbPermissionRegistry.setSource(SOURCE, [allow(CREATE)]);

			expect(context.forbidden.getStates()).to.deep.equal([{ unique: 'other' }]);
		});
	});

	it('changes nothing before the workspace knows whether the entity is new', async () => {
		await createController();

		expect(permissionRule()).to.be.undefined;
		expect(forbiddenState()).to.be.undefined;
	});

	it('applies the create verb once an existing entity is replaced by a new one', async () => {
		umbPermissionRegistry.setSource(SOURCE, [allow(UPDATE), deny(CREATE, LOCKED)]);
		context.setNew(false);
		await createController();

		context.resetState();
		context.setNew(true);

		expect(forbiddenState()?.reason).to.deep.equal(LOCKED);
	});

	it('applies the update verb once a new entity is replaced by an existing one', async () => {
		umbPermissionRegistry.setSource(SOURCE, [deny(CREATE, LOCKED), deny(UPDATE, ARCHIVED)]);
		context.setNew(true);
		await createController();

		context.resetState();
		context.setNew(false);

		expect(forbiddenState()).to.be.undefined;
		expect(permissionRule()?.message).to.equal(ARCHIVED.title);
	});

	it('applies again after the workspace state is reset for another entity', async () => {
		umbPermissionRegistry.setSource(SOURCE, [deny(UPDATE, LOCKED)]);
		context.setNew(false);
		await createController();

		context.resetState();
		expect(permissionRule()).to.be.undefined;
		context.setNew(false);

		expect(permissionRule()?.message).to.equal(LOCKED.title);
	});

	it('removes its read-only rule when destroyed', async () => {
		context.setNew(false);
		const controller = await createController();

		controller.destroy();

		expect(permissionRule()).to.be.undefined;
	});

	it('removes its forbidden state when destroyed', async () => {
		context.setNew(true);
		const controller = await createController();

		controller.destroy();

		expect(forbiddenState()).to.be.undefined;
	});

	it('stops applying after it is destroyed', async () => {
		context.setNew(false);
		const controller = await createController();
		controller.destroy();

		umbPermissionRegistry.setSource(SOURCE, [deny(UPDATE, LOCKED)]);

		expect(permissionRule()).to.be.undefined;
	});
});
