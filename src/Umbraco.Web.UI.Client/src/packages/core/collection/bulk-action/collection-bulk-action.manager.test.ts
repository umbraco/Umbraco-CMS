import { UmbCollectionBulkActionManager } from './collection-bulk-action.manager.js';
import { umbExtensionsRegistry } from '../../extension-registry/index.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('test-bulk-action-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const bulkActionManifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.Test.EntityBulkAction.1',
		name: 'Test Entity Bulk Action 1',
		forEntityTypes: ['test-entity'],
	},
];

describe('UmbCollectionBulkActionManager', () => {
	let hostElement: UmbTestControllerHostElement;
	let manager: UmbCollectionBulkActionManager;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		manager = new UmbCollectionBulkActionManager(hostElement);
	});

	afterEach(() => {
		manager.destroy();
		hostElement.destroy();
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a hasBulkActions property', () => {
				expect(manager).to.have.property('hasBulkActions').to.be.an.instanceOf(Observable);
			});
		});
	});

	describe('hasBulkActions', () => {
		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		it('it emits false if there are no actions', (done) => {
			// Need to call setConfig to initialize the observer
			manager.setConfig({ enabled: true });

			manager.hasBulkActions.subscribe((value) => {
				expect(value).to.equal(false);
				done();
			});
		});

		it('it emits true if there are actions', (done) => {
			// First, we need to add bulk action manifests to the registry
			umbExtensionsRegistry.registerMany(bulkActionManifests);

			// Need to call setConfig to initialize the observer
			manager.setConfig({ enabled: true });

			manager.hasBulkActions.subscribe((value) => {
				if (value === true) {
					expect(value).to.equal(true);
					done();
				}
			});
		});
	});
});
