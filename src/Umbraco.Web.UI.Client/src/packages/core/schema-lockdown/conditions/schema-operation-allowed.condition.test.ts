import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '../schema-lockdown.context-token.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '../constants.js';
import type { UmbSchemaOperation } from '../types.js';
import { UmbSchemaOperationAllowedCondition } from './schema-operation-allowed.condition.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('test-controller-host-schema-operation-allowed')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-child-schema-operation-allowed')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

const LOCKED_ENTITY_TYPE = 'document-type';

describe('UmbSchemaOperationAllowedCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let deniedOperations: UmbObjectState<Array<UmbSchemaOperation>>;
	let condition: UmbSchemaOperationAllowedCondition;

	const createCondition = (config: { entityType?: string; operation: UmbSchemaOperation; match?: boolean }) =>
		new UmbSchemaOperationAllowedCondition(childElement, {
			host: childElement,
			config: { alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS, ...config },
			onChange: () => {},
		});

	const provideWorkspace = (entityType: string) => {
		const workspaceContext = {
			IS_ENTITY_DETAIL_WORKSPACE_CONTEXT: true,
			getEntityType: () => entityType,
			getHostElement: () => hostElement,
		};
		new UmbContextProvider(hostElement, UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, workspaceContext as never).hostConnected();
	};

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		deniedOperations = new UmbObjectState<Array<UmbSchemaOperation>>(['update']);
		const schemaLockdownContext = {
			state: deniedOperations.asObservable(),
			isAllowed: (entityType: string, operation: UmbSchemaOperation) =>
				entityType !== LOCKED_ENTITY_TYPE || !deniedOperations.getValue().includes(operation),
			getHostElement: () => hostElement,
		};
		new UmbContextProvider(hostElement, UMB_SCHEMA_LOCKDOWN_CONTEXT, schemaLockdownContext as never).hostConnected();
	});

	afterEach(() => {
		condition?.hostDisconnected();
		document.body.innerHTML = '';
	});

	it('is permitted while the operation is allowed, by default', async () => {
		condition = createCondition({ entityType: LOCKED_ENTITY_TYPE, operation: 'create' });

		await waitUntil(() => condition.permitted, 'the condition was not permitted');
	});

	it('is not permitted while the operation is denied, by default', async () => {
		condition = createCondition({ entityType: LOCKED_ENTITY_TYPE, operation: 'update' });
		await aTimeout(0);

		expect(condition.permitted).to.be.false;
	});

	it('is permitted while the operation is denied when matching false', async () => {
		condition = createCondition({ entityType: LOCKED_ENTITY_TYPE, operation: 'update', match: false });

		await waitUntil(() => condition.permitted, 'the condition was not permitted');
	});

	it('follows the lockdown when it changes', async () => {
		condition = createCondition({ entityType: LOCKED_ENTITY_TYPE, operation: 'update', match: false });
		await waitUntil(() => condition.permitted, 'the condition was not permitted');

		deniedOperations.setValue([]);

		await waitUntil(() => !condition.permitted, 'the condition stayed permitted once the operation was allowed');
	});

	it('checks the entity type of the surrounding workspace when no entity type is configured', async () => {
		provideWorkspace(LOCKED_ENTITY_TYPE);
		condition = createCondition({ operation: 'update', match: false });

		await waitUntil(() => condition.permitted, 'the condition was not permitted');
	});

	it('checks the configured entity type rather than the one of the surrounding workspace', async () => {
		provideWorkspace(LOCKED_ENTITY_TYPE);
		condition = createCondition({ entityType: 'data-type', operation: 'update', match: false });
		await aTimeout(0);

		expect(condition.permitted).to.be.false;
	});
});
