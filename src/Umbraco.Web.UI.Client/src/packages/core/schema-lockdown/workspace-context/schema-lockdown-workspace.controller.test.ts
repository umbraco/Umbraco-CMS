import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '../schema-lockdown.context-token.js';
import type { UmbSchemaOperation } from '../types.js';
import { UmbSchemaLockdownWorkspaceController } from './schema-lockdown-workspace.controller.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbReadOnlyGuardManager, UmbStateManager, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-test-schema-lockdown-workspace-host')
class UmbTestSchemaLockdownWorkspaceHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbSchemaLockdownWorkspaceController', () => {
	let host: UmbTestSchemaLockdownWorkspaceHostElement;
	let lockdownState: UmbObjectState<{ restrictedEntityTypes: Array<unknown> }>;
	let allowedOperations: Record<UmbSchemaOperation, boolean>;
	let entityReadOnlyGuard: UmbReadOnlyGuardManager<UmbGuardRule>;
	let forbidden: UmbStateManager;
	let unique: UmbObjectState<string | null>;

	const setup = (entityType: string, isNew = false) => {
		unique = new UmbObjectState<string | null>(isNew ? null : 'existing');
		const workspaceContext = {
			unique: unique.asObservable(),
			isNew: new UmbBooleanState(isNew).asObservable(),
			getEntityType: () => entityType,
			requestSubmit: () => Promise.resolve(),
			IS_ENTITY_DETAIL_WORKSPACE_CONTEXT: true,
			readOnlyGuard: entityReadOnlyGuard,
			forbidden,
			getHostElement: () => host,
		};
		new UmbContextProviderController(host, UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, workspaceContext as never);
		return new UmbSchemaLockdownWorkspaceController(host);
	};

	beforeEach(() => {
		host = new UmbTestSchemaLockdownWorkspaceHostElement();
		document.body.appendChild(host);

		allowedOperations = { create: true, update: true, delete: true };
		lockdownState = new UmbObjectState({ restrictedEntityTypes: [] as Array<unknown> });
		const schemaLockdownContext = {
			state: lockdownState.asObservable(),
			isAllowed: (entityType: string, operation: UmbSchemaOperation) =>
				entityType === 'document-type' ? allowedOperations[operation] : true,
			getHostElement: () => host,
		};
		new UmbContextProviderController(host, UMB_SCHEMA_LOCKDOWN_CONTEXT, schemaLockdownContext as never);

		entityReadOnlyGuard = new UmbReadOnlyGuardManager<UmbGuardRule>(host);
		forbidden = new UmbStateManager(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('makes the entity read-only while saving it is not allowed', async () => {
		allowedOperations = { create: true, update: false, delete: true };
		setup('document-type');

		await waitUntil(() => entityReadOnlyGuard.getPermitted(), 'the entity was not made read-only');
		expect(entityReadOnlyGuard.getRules().length).to.equal(1);
	});

	it('makes the entity editable again once saving it is allowed', async () => {
		allowedOperations = { create: true, update: false, delete: true };
		setup('document-type');
		await waitUntil(() => entityReadOnlyGuard.getPermitted(), 'the entity was not made read-only');

		allowedOperations = { create: true, update: true, delete: true };
		lockdownState.setValue({ restrictedEntityTypes: [{}] });

		await waitUntil(() => !entityReadOnlyGuard.getPermitted(), 'the entity stayed read-only');
	});

	it('makes another loaded entity read-only again after the workspace clears its rules', async () => {
		allowedOperations = { create: true, update: false, delete: true };
		setup('document-type');
		await waitUntil(() => entityReadOnlyGuard.getPermitted(), 'the entity was not made read-only');

		entityReadOnlyGuard.clearRules();
		unique.setValue('another');

		await waitUntil(() => entityReadOnlyGuard.getPermitted(), 'the other entity was not made read-only');
	});

	it('does not forbid an existing entity that cannot be updated', async () => {
		allowedOperations = { create: false, update: false, delete: true };
		setup('document-type');
		await waitUntil(() => entityReadOnlyGuard.getPermitted(), 'the entity was not made read-only');

		expect(forbidden.getIsOn()).to.be.false;
	});

	it('forbids a new entity while creating it is not allowed', async () => {
		allowedOperations = { create: false, update: true, delete: true };
		setup('document-type', true);

		await waitUntil(() => forbidden.getIsOn(), 'the new entity was not forbidden');
		expect(entityReadOnlyGuard.getPermitted()).to.be.true;
	});

	it('keeps a new entity editable while it can be created, even if it cannot be updated', async () => {
		allowedOperations = { create: true, update: false, delete: true };
		setup('document-type', true);
		await aTimeout(0);

		expect(entityReadOnlyGuard.getPermitted()).to.be.false;
		expect(forbidden.getIsOn()).to.be.false;
	});

	it('forbids a new entity again after its forbidden state is cleared', async () => {
		allowedOperations = { create: false, update: true, delete: true };
		setup('document-type', true);
		await waitUntil(() => forbidden.getIsOn(), 'the new entity was not forbidden');

		forbidden.clear();

		await waitUntil(() => forbidden.getIsOn(), 'the new entity was not forbidden again');
	});

	it('stops forbidding a new entity once creating it is allowed', async () => {
		allowedOperations = { create: false, update: true, delete: true };
		setup('document-type', true);
		await waitUntil(() => forbidden.getIsOn(), 'the new entity was not forbidden');

		allowedOperations = { create: true, update: true, delete: true };
		lockdownState.setValue({ restrictedEntityTypes: [{}] });

		await waitUntil(() => !forbidden.getIsOn(), 'the new entity stayed forbidden');
	});

	it('leaves workspaces of other entity types untouched', async () => {
		allowedOperations = { create: false, update: false, delete: false };
		setup('document', true);
		await aTimeout(0);

		expect(entityReadOnlyGuard.getRules()).to.be.empty;
		expect(forbidden.getIsOn()).to.be.false;
	});
});
