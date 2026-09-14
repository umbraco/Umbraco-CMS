import type { ManifestCollectionView } from './collection-view.extension.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbCollectionViewManager } from './collection-view.manager.js';
import { aTimeout, expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

const CURRENT_VIEW_MEMORY_UNIQUE = 'UmbCollectionCurrentView';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const VIEW_1_ALIAS = 'UmbTest.CollectionView.1';
const VIEW_2_ALIAS = 'UmbTest.CollectionView.2';

const views: Array<ManifestCollectionView> = [
	{
		type: 'collectionView',
		alias: VIEW_1_ALIAS,
		name: '1',
		meta: {
			label: '1',
			icon: 'icon-list',
			pathName: '1',
		},
	},
	{
		type: 'collectionView',
		alias: VIEW_2_ALIAS,
		name: '2',
		meta: {
			label: '2',
			icon: 'icon-list',
			pathName: '2',
		},
	},
];

umbExtensionsRegistry.registerMany(views);

describe('UmbCollectionViewManager', () => {
	let manager: UmbCollectionViewManager;
	const config = { defaultViewAlias: VIEW_2_ALIAS };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbCollectionViewManager(hostElement);
		manager.setConfig(config);
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

			it('has a rootPathName property', () => {
				expect(manager).to.have.property('rootPathName').to.be.an.instanceOf(Observable);
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

	describe('Current view', () => {
		it('sets and gets the current view value', () => {
			manager.setCurrentView(views[0]);
			expect(manager.getCurrentView()?.alias).to.equal(views[0].alias);
		});

		it('updates the observable', (done) => {
			manager.setCurrentView(views[0]);

			manager.currentView.subscribe((value) => {
				setTimeout(() => {
					expect(value).to.deep.equal(views[0]);
					done();
				}, 60);
			});
		});
	});

	describe('Interaction memory', () => {
		let hostElement: UmbTestControllerHostElement;
		let interactionMemory: UmbInteractionMemoryManager;

		const getMemorizedViewAlias = () => interactionMemory.getMemory(CURRENT_VIEW_MEMORY_UNIQUE)?.value?.alias;

		const createManager = () => {
			const memorizingManager = new UmbCollectionViewManager(hostElement, {
				interactionMemoryManager: interactionMemory,
			});
			memorizingManager.setConfig(config);
			return memorizingManager;
		};

		const getLandingViewAlias = async (memorizingManager: UmbCollectionViewManager) => {
			let routes: Array<UmbRoute> = [];
			const subscription = memorizingManager.routes.subscribe((value) => (routes = value));
			await aTimeout(100);
			subscription.unsubscribe();
			return routes.find((route) => route.path === '')?.unique;
		};

		beforeEach(() => {
			hostElement = new UmbTestControllerHostElement();
			interactionMemory = new UmbInteractionMemoryManager(hostElement);
		});

		it('remembers the current view', () => {
			createManager().setCurrentView(views[0]);
			expect(getMemorizedViewAlias()).to.equal(VIEW_1_ALIAS);
		});

		it('lands on the remembered view instead of the configured default view', async () => {
			interactionMemory.setMemory({
				unique: CURRENT_VIEW_MEMORY_UNIQUE,
				value: { alias: VIEW_1_ALIAS },
			});
			expect(await getLandingViewAlias(createManager())).to.equal(VIEW_1_ALIAS);
		});

		it('lands on the configured default view when nothing is remembered', async () => {
			expect(await getLandingViewAlias(createManager())).to.equal(VIEW_2_ALIAS);
		});

		it('lands on the remembered view when it is remembered after the views have loaded', async () => {
			const memorizingManager = createManager();
			await aTimeout(100);
			interactionMemory.setMemory({
				unique: CURRENT_VIEW_MEMORY_UNIQUE,
				value: { alias: VIEW_1_ALIAS },
			});
			expect(await getLandingViewAlias(memorizingManager)).to.equal(VIEW_1_ALIAS);
		});

		it('does not remember a view when no interaction memory is given', () => {
			manager.setCurrentView(views[0]);
			expect(getMemorizedViewAlias()).to.be.undefined;
		});
	});

	/* TODO: look into why these test dosn't wait for the observable to update
	describe('Views', () => {
		it('updates the observable', (done) => {
			manager.views.subscribe((value) => {
				setTimeout(() => {
					expect(value).to.have.lengthOf(2);
					done();
				}, 60);
			});
		});
	});

	describe('Routes', () => {
		it('updates the observable', (done) => {
			manager.routes.subscribe((value) => {
				setTimeout(() => {
					expect(value).to.have.lengthOf(3);
					done();
				}, 60);
			});
		});

		it('includes the views as routes', (done) => {
			manager.routes.subscribe((value) => {
				setTimeout(() => {
					expect(value[0].path).to.equal(views[0].meta.pathName);
					expect(value[1].path).to.equal(views[1].meta.pathName);
					done();
				}, 60);
			});
		});

		it('has a catch all route to the default view', (done) => {
			manager.routes.subscribe((value) => {
				setTimeout(() => {
					expect(value[2].path).to.equal('');
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					// TODO: Fix this type error
					expect(value[2].redirectTo).to.equal(config.defaultViewAlias);
					done();
				}, 60);
			});
		});
	});
	*/
});
