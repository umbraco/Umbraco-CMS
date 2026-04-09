import { UmbPropertySortModeContext } from './property-sort-mode.context.js';
import { UMB_PROPERTY_SORT_MODE_CONTEXT } from './property-sort-mode.context-token.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-test-host')
export class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

@customElement('umb-test-child')
export class UmbTestChildElement extends UmbElementMixin(HTMLElement) {}

describe('UmbPropertySortModeContext', () => {
	let context: UmbPropertySortModeContext;
	let host: UmbTestHostElement;
	let child: UmbTestChildElement;

	beforeEach(() => {
		host = new UmbTestHostElement();
		child = new UmbTestChildElement();
		host.appendChild(child);
		document.body.appendChild(host);
		context = new UmbPropertySortModeContext(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has an isSortMode property', () => {
				expect(context).to.have.property('isSortMode').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a getIsSortMode method', () => {
				expect(context).to.have.property('getIsSortMode').that.is.a('function');
			});

			it('has a setIsSortMode method', () => {
				expect(context).to.have.property('setIsSortMode').that.is.a('function');
			});

			it('has a toggleIsSortMode method', () => {
				expect(context).to.have.property('toggleIsSortMode').that.is.a('function');
			});
		});
	});

	describe('default value', () => {
		it('should return false by default', () => {
			expect(context.getIsSortMode()).to.equal(false);
		});
	});

	describe('setIsSortMode', () => {
		it('should set sort mode to true', () => {
			context.setIsSortMode(true);
			expect(context.getIsSortMode()).to.equal(true);
		});

		it('should set sort mode to false', () => {
			context.setIsSortMode(true);
			context.setIsSortMode(false);
			expect(context.getIsSortMode()).to.equal(false);
		});
	});

	describe('toggleIsSortMode', () => {
		it('should toggle from false to true', () => {
			expect(context.getIsSortMode()).to.equal(false);
			context.toggleIsSortMode();
			expect(context.getIsSortMode()).to.equal(true);
		});

		it('should toggle from true to false', () => {
			context.setIsSortMode(true);
			context.toggleIsSortMode();
			expect(context.getIsSortMode()).to.equal(false);
		});
	});

	describe('context provision', () => {
		it('should be provided as a context', async () => {
			const providedContext = await child.getContext(UMB_PROPERTY_SORT_MODE_CONTEXT);
			expect(providedContext).to.equal(context);
		});
	});
});
