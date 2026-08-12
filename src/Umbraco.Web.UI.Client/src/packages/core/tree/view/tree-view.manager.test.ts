import type { ManifestTreeView } from './tree-view.extension.js';
import { UmbTreeViewManager } from './tree-view.manager.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-tree-view-manager-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const TREE_ALIAS = 'UmbTest.Tree.ViewManager';
const VIEW_LOW_ALIAS = 'UmbTest.TreeView.LowWeight';
const VIEW_HIGH_ALIAS = 'UmbTest.TreeView.HighWeight';

const testViews: Array<ManifestTreeView> = [
	{
		type: 'treeView',
		alias: VIEW_LOW_ALIAS,
		name: 'Low Weight View',
		weight: 100,
		meta: { label: 'Low', icon: 'icon-list' },
		forTrees: [TREE_ALIAS],
	},
	{
		type: 'treeView',
		alias: VIEW_HIGH_ALIAS,
		name: 'High Weight View',
		weight: 900,
		meta: { label: 'High', icon: 'icon-grid' },
		forTrees: [TREE_ALIAS],
	},
];

umbExtensionsRegistry.registerMany(testViews);

describe('UmbTreeViewManager', () => {
	let manager: UmbTreeViewManager;

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbTreeViewManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a views property', () => {
				expect(manager).to.have.property('views').to.be.an.instanceOf(Observable);
			});

			it('has a currentView property', () => {
				expect(manager).to.have.property('currentView').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a setTreeAlias method', () => {
				expect(manager).to.have.property('setTreeAlias').that.is.a('function');
			});

			it('has a setCurrentView method', () => {
				expect(manager).to.have.property('setCurrentView').that.is.a('function');
			});

			it('has a getCurrentView method', () => {
				expect(manager).to.have.property('getCurrentView').that.is.a('function');
			});
		});
	});

	describe('setCurrentView / getCurrentView', () => {
		it('sets and returns the active view', () => {
			manager.setCurrentView(testViews[0]);
			expect(manager.getCurrentView()?.alias).to.equal(VIEW_LOW_ALIAS);
		});

		it('updates the currentView observable', (done) => {
			manager.setCurrentView(testViews[1]);
			manager.currentView.subscribe((value) => {
				if (value?.alias === VIEW_HIGH_ALIAS) {
					expect(value.alias).to.equal(VIEW_HIGH_ALIAS);
					done();
				}
			});
		});
	});

	describe('setTreeAlias', () => {
		it('populates views from the extension registry for the given alias', (done) => {
			manager.setTreeAlias(TREE_ALIAS);
			manager.views.subscribe((views) => {
				if (views.length >= 2) {
					expect(views).to.have.lengthOf(2);
					done();
				}
			});
		});

		it('selects the highest-weight view as the default current view', (done) => {
			manager.setTreeAlias(TREE_ALIAS);
			manager.currentView.subscribe((view) => {
				if (view?.alias === VIEW_HIGH_ALIAS) {
					expect(view.alias).to.equal(VIEW_HIGH_ALIAS);
					done();
				}
			});
		});

		it('falls back to the built-in classic view when no manifests match the alias', (done) => {
			manager.setTreeAlias('UmbTest.Tree.NoViewsRegistered');
			manager.currentView.subscribe((view) => {
				if (view) {
					expect(view.alias).to.equal('Umb.TreeView.Classic.Fallback');
					done();
				}
			});
		});
	});
});
