import { UmbPropertySortModePropertyAction } from './property-sort-mode-property-action.js';
import { UmbPropertySortModeContext } from '../property-context/property-sort-mode.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-test-host')
export class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbPropertySortModePropertyAction', () => {
	let action: UmbPropertySortModePropertyAction;
	let host: UmbTestHostElement;
	let context: UmbPropertySortModeContext;

	beforeEach(() => {
		host = new UmbTestHostElement();
		document.body.appendChild(host);
		context = new UmbPropertySortModeContext(host);
		//@ts-ignore - TODO: investigate Property Action args type. 'never' doesn't seem correct.
		action = new UmbPropertySortModePropertyAction(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		it('has an execute method', () => {
			expect(action).to.have.property('execute').that.is.a('function');
		});
	});

	describe('execute', () => {
		it('should toggle sort mode from false to true', async () => {
			expect(context.getIsSortMode()).to.equal(false);
			await action.execute();
			expect(context.getIsSortMode()).to.equal(true);
		});

		it('should toggle sort mode from true to false', async () => {
			context.setIsSortMode(true);
			expect(context.getIsSortMode()).to.equal(true);
			await action.execute();
			expect(context.getIsSortMode()).to.equal(false);
		});

		it('should toggle back and forth on multiple executions', async () => {
			expect(context.getIsSortMode()).to.equal(false);
			await action.execute();
			expect(context.getIsSortMode()).to.equal(true);
			await action.execute();
			expect(context.getIsSortMode()).to.equal(false);
			await action.execute();
			expect(context.getIsSortMode()).to.equal(true);
		});
	});
});
