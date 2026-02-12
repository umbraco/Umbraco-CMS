import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeItemActiveManager } from './tree-active-manager';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTreeItemActiveManager', () => {
	let manager: UmbTreeItemActiveManager;
	const item = { entityType: 'test', unique: '123' };
	const item2 = { entityType: 'test', unique: '456' };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbTreeItemActiveManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has an active property', () => {
				expect(manager).to.have.property('active').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has an isActive method', () => {
				expect(manager).to.have.property('isActive').that.is.a('function');
			});
		});
	});

	describe('isActive', () => {
		it('checks if an item is active', (done) => {
			manager.setActive([item]);
			const isActive = manager.isActive(item);
			expect(isActive).to.be.an.instanceOf(Observable);
			manager.isActive(item).subscribe((value) => {
				expect(value).to.be.true;
				done();
			});
		});
	});

	describe('setActive & getActive', () => {
		it('sets and gets the active state', () => {
			const active = [item];
			manager.setActive(active);
			expect(manager.getActive()).to.deep.equal(active);
		});
	});

	describe('removeActiveIfMatch', () => {
		it('removes the active state', () => {
			const active = [item];
			manager.setActive(active);
			manager.removeActiveIfMatch(active);
			expect(manager.getActive()).to.deep.equal([]);
		});
		it('does not remove the active state if it does not match', () => {
			const active = [item];
			manager.setActive(active);
			manager.removeActiveIfMatch([item2]);
			expect(manager.getActive()).to.deep.equal([item]);
		});
	});
});
