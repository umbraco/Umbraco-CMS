import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionPickerDataSource } from './extension.picker-data-source.js';
import type { UmbExtensionPickerDataSourceConfigCollectionModel } from './types.js';
import type { UmbExtensionCollectionFilterModel } from '../collection/types.js';

@customElement('test-extension-picker-data-source-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const testExtensions = [
	{ type: 'section', alias: 'Test.Section.One', name: 'Section One' },
	{ type: 'section', alias: 'Test.Section.Two', name: 'Section Two' },
	{ type: 'dashboard', alias: 'Test.Dashboard.One', name: 'Dashboard One' },
	{ type: 'propertyEditorUi', alias: 'Test.PropertyEditorUi.One', name: 'Property Editor UI One' },
	{ type: 'workspace', alias: 'Test.Workspace.One', name: 'Workspace One' },
];

describe('UmbExtensionPickerDataSource', () => {
	let hostElement: UmbTestControllerHostElement;
	let dataSource: UmbExtensionPickerDataSource;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		umbExtensionsRegistry.registerMany(testExtensions as any);

		dataSource = new UmbExtensionPickerDataSource(hostElement);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(testExtensions.map((ext) => ext.alias));
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		it('has a setConfig method', () => {
			expect(dataSource).to.have.property('setConfig').that.is.a('function');
		});

		it('has a getConfig method', () => {
			expect(dataSource).to.have.property('getConfig').that.is.a('function');
		});

		it('has a requestCollection method', () => {
			expect(dataSource).to.have.property('requestCollection').that.is.a('function');
		});

		it('has a requestItems method', () => {
			expect(dataSource).to.have.property('requestItems').that.is.a('function');
		});
	});

	describe('setConfig / getConfig', () => {
		it('returns undefined when no config is set', () => {
			expect(dataSource.getConfig()).to.be.undefined;
		});

		it('stores and returns the config', () => {
			const config: UmbExtensionPickerDataSourceConfigCollectionModel = [
				{ alias: 'allowedExtensionTypes', value: ['section'] },
			];
			dataSource.setConfig(config);
			expect(dataSource.getConfig()).to.equal(config);
		});

		it('can clear the config by setting undefined', () => {
			const config: UmbExtensionPickerDataSourceConfigCollectionModel = [
				{ alias: 'allowedExtensionTypes', value: ['section'] },
			];
			dataSource.setConfig(config);
			dataSource.setConfig(undefined);
			expect(dataSource.getConfig()).to.be.undefined;
		});
	});

	describe('requestCollection', () => {
		it('returns all extensions when no config and no type filter', async () => {
			const args: UmbExtensionCollectionFilterModel = {};
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			expect(result.data!.items.length).to.be.greaterThanOrEqual(testExtensions.length);
		});

		it('passes through requested types when no config restriction is set', async () => {
			const args: UmbExtensionCollectionFilterModel = { extensionTypes: ['section'] };
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			const types = result.data!.items.map((item) => item.manifest.type);
			types.forEach((type) => expect(type).to.equal('section'));
		});

		it('filters by allowed types from config when no requested types', async () => {
			dataSource.setConfig([{ alias: 'allowedExtensionTypes', value: ['dashboard'] }]);
			const args: UmbExtensionCollectionFilterModel = {};
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			const types = result.data!.items.map((item) => item.manifest.type);
			types.forEach((type) => expect(type).to.equal('dashboard'));
		});

		it('intersects requested types with allowed config types', async () => {
			dataSource.setConfig([{ alias: 'allowedExtensionTypes', value: ['section', 'dashboard'] }]);
			const args: UmbExtensionCollectionFilterModel = { extensionTypes: ['section', 'workspace'] };
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			const types = result.data!.items.map((item) => item.manifest.type);
			types.forEach((type) => expect(type).to.equal('section'));
			expect(types).to.not.include('workspace');
		});

		it('produces an empty extensionTypes array when requested types do not overlap with config allowed types', async () => {
			dataSource.setConfig([{ alias: 'allowedExtensionTypes', value: ['dashboard'] }]);
			const args: UmbExtensionCollectionFilterModel = { extensionTypes: ['workspace'] };
			const result = await dataSource.requestCollection(args);
			// The intersection is empty, so the repository receives an empty array.
			// The repository treats an empty array as "no type filter", returning all extensions.
			expect(result.data).to.not.be.undefined;
			expect(result.data!.items.length).to.be.greaterThan(0);
		});

		it('does not clear requested types when config is undefined', async () => {
			dataSource.setConfig(undefined);
			const args: UmbExtensionCollectionFilterModel = { extensionTypes: ['section'] };
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			const types = result.data!.items.map((item) => item.manifest.type);
			expect(types.length).to.be.greaterThan(0);
			types.forEach((type) => expect(type).to.equal('section'));
		});

		it('does not clear requested types when config has empty allowed types', async () => {
			dataSource.setConfig([{ alias: 'allowedExtensionTypes', value: [] }]);
			const args: UmbExtensionCollectionFilterModel = { extensionTypes: ['dashboard'] };
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			const types = result.data!.items.map((item) => item.manifest.type);
			expect(types.length).to.be.greaterThan(0);
			types.forEach((type) => expect(type).to.equal('dashboard'));
		});

		it('preserves other filter args like skip and take', async () => {
			const args: UmbExtensionCollectionFilterModel = { skip: 0, take: 2 };
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			expect(result.data!.items.length).to.be.at.most(2);
		});

		it('supports text filter', async () => {
			const args: UmbExtensionCollectionFilterModel = { filter: 'Dashboard' };
			const result = await dataSource.requestCollection(args);
			expect(result.data).to.not.be.undefined;
			result.data!.items.forEach((item) => {
				const matchesName = item.name?.toLowerCase().includes('dashboard') ?? false;
				const matchesAlias = item.manifest.alias.toLowerCase().includes('dashboard');
				expect(matchesName || matchesAlias).to.be.true;
			});
		});
	});
});
