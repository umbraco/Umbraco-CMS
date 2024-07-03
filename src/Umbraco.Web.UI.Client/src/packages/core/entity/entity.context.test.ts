import { UmbEntityContext } from './entity.context.js';
import { UMB_ENTITY_CONTEXT } from './entity.context-token.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-test-host')
export class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

@customElement('umb-test-child')
export class UmbTestChildElement extends UmbElementMixin(HTMLElement) {}

describe('UmbEntityContext', () => {
	let context: UmbEntityContext;
	let host: UmbTestHostElement;
	let child: UmbTestChildElement;

	beforeEach(() => {
		host = new UmbTestHostElement();
		child = new UmbTestChildElement();
		host.appendChild(child);
		document.body.appendChild(host);
		context = new UmbEntityContext(host);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a entity type property', () => {
				expect(context).to.have.property('entityType').to.be.an.instanceOf(Observable);
			});

			it('has a unique property', () => {
				expect(context).to.have.property('unique').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a getEntityType method', () => {
				expect(context).to.have.property('getEntityType').that.is.a('function');
			});

			it('has a setEntityType method', () => {
				expect(context).to.have.property('setEntityType').that.is.a('function');
			});

			it('has a getUnique method', () => {
				expect(context).to.have.property('getUnique').that.is.a('function');
			});

			it('has a setUnique method', () => {
				expect(context).to.have.property('setUnique').that.is.a('function');
			});
		});
	});

	describe('set and get entity type', () => {
		it('should set entity type', () => {
			context.setEntityType('entity-type');
			expect(context.getEntityType()).to.equal('entity-type');
		});
	});

	describe('set and get unique', () => {
		it('should set unique', () => {
			context.setUnique('unique-value');
			expect(context.getUnique()).to.equal('unique-value');
		});
	});

	describe('it is provided as a context', () => {
		it('should be provided as a context', async () => {
			const providedContext = await child.getContext(UMB_ENTITY_CONTEXT);
			expect(providedContext).to.equal(context);
		});
	});
});
