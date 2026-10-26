import { UmbSchemaLockdownContext } from './schema-lockdown.context.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-test-schema-lockdown-host')
export class UmbTestSchemaLockdownHostElement extends UmbElementMixin(HTMLElement) {}

// No mock handler is registered for the schema lockdown endpoint, so the context's initial request always fails
// here - which is the scenario these tests are about.
describe('UmbSchemaLockdownContext', () => {
	let host: UmbTestSchemaLockdownHostElement;
	let context: UmbSchemaLockdownContext;

	beforeEach(() => {
		host = new UmbTestSchemaLockdownHostElement();
		document.body.appendChild(host);
		context = new UmbSchemaLockdownContext(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('permits an operation while the matrix has not been retrieved', () => {
		expect(context.isAllowed(UMB_DOCUMENT_TYPE_ENTITY_TYPE, 'update')).to.be.true;
	});

	it('permits an operation after retrieving the matrix has failed', async () => {
		await aTimeout(100);

		expect(context.isAllowed(UMB_DOCUMENT_TYPE_ENTITY_TYPE, 'update')).to.be.true;
	});

	it('permits an operation on an entity type outside the matrix', () => {
		expect(context.isAllowed('an-entity-type-schema-lockdown-does-not-govern', 'delete')).to.be.true;
	});
});
