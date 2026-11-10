import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from './schema-lockdown.context-token.js';
import { UmbSchemaOperationAllowedController } from './schema-operation-allowed.controller.js';
import type { UmbSchemaOperation } from './types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-schema-operation-allowed-host')
export class UmbTestSchemaOperationAllowedHostElement extends UmbElementMixin(HTMLElement) {}

const ENTITY_TYPE = 'test-entity-type';

describe('UmbSchemaOperationAllowedController', () => {
	let host: UmbTestSchemaOperationAllowedHostElement;
	let allowedOperations: Record<UmbSchemaOperation, boolean>;

	beforeEach(() => {
		host = new UmbTestSchemaOperationAllowedHostElement();
		document.body.appendChild(host);

		allowedOperations = { create: true, update: false, delete: false };
		const schemaLockdownContext = {
			state: new UmbObjectState({ restrictedEntityTypes: [] }).asObservable(),
			isAllowed: (_entityType: string, operation: UmbSchemaOperation) => allowedOperations[operation],
			getHostElement: () => host,
		};
		new UmbContextProviderController(host, UMB_SCHEMA_LOCKDOWN_CONTEXT, schemaLockdownContext as never);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('checks the given operation', async () => {
		const create = new UmbSchemaOperationAllowedController(host, { entityType: ENTITY_TYPE, operation: 'create' });
		const update = new UmbSchemaOperationAllowedController(host, { entityType: ENTITY_TYPE, operation: 'update' });
		await aTimeout(0);

		expect(create.getAllowed()).to.be.true;
		expect(update.getAllowed()).to.be.false;
	});

	it('allows the operation until the entity type is known', async () => {
		const controller = new UmbSchemaOperationAllowedController(host, { operation: 'update' });
		await aTimeout(0);

		expect(controller.getAllowed()).to.be.true;

		controller.setEntityType(ENTITY_TYPE);

		expect(controller.getAllowed()).to.be.false;
	});

	it('can be destroyed once it has consumed its context', async () => {
		const controller = new UmbSchemaOperationAllowedController(host, { entityType: ENTITY_TYPE, operation: 'update' });
		await aTimeout(0);

		expect(() => controller.destroy()).not.to.throw();
	});
});
