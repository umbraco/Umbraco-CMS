import { expect } from '@open-wc/testing';
import { UmbCollectionViewManager } from './collection-view.manager.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { extensionRegistry } from '@umbraco-cms/backoffice/extension-registry';

class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

extensionRegistry;

describe('UmbCollectionViewManager', () => {
	let manager: UmbCollectionViewManager;

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbCollectionViewManager(hostElement, {});
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a views property', () => {
				expect(manager).to.have.property('views').to.be.an.instanceOf(Observable);
			});

			it('has a currentView property', () => {
				expect(manager).to.have.property('currentView').to.be.an.instanceOf(Observable);
			});

			it('has a routes property', () => {
				expect(manager).to.have.property('routes').to.be.an.instanceOf(Observable);
			});

			it('has a rootPathname property', () => {
				expect(manager).to.have.property('rootPathname').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a setCurrentView method', () => {
				expect(manager).to.have.property('setCurrentView').that.is.a('function');
			});

			it('has a getCurrentView method', () => {
				expect(manager).to.have.property('getCurrentView').that.is.a('function');
			});
		});
	});
});
