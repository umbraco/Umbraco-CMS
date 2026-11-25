import { UmbDataTypeDetailStore } from '../repository/detail/data-type-detail.store.js';
import { manifests as dataTypeDetailRepositoryManifests } from '../repository/detail/manifests.js';
import { umbDataTypeMockDb } from '../../../../mocks/db/data-type.db.js';
import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import type {
	UmbPropertyEditorConfig,
	UmbPropertyEditorConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-test-data-type-workspace-host')
class UmbTestDataTypeWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbActionEventContext(this);
		new UmbDataTypeDetailStore(this);
	}
}

class UmbTestRenamingConverter extends UmbControllerBase implements UmbPropertyEditorConfigConverter {
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
		return [{ alias: 'newName', value: config.find((x) => x.alias === 'oldName')?.value }];
	}
}

class UmbTestFailingConverter extends UmbControllerBase implements UmbPropertyEditorConfigConverter {
	convert(): UmbPropertyEditorConfig {
		throw new Error('Test converter failure');
	}
}

const DATA_TYPE_ID = 'data-type-workspace-config-converter-test';

// Three property editors: "From" and "To" name a setting differently and have a converter between them; "Other"
// shares only one setting with "From" and has no converter. A converter that fails is registered for "Failing".
const editor = (name: string, settings: Array<string>) => [
	{
		type: 'propertyEditorSchema',
		alias: `Test.${name}`,
		name: `Test ${name} Schema`,
		meta: {
			defaultPropertyEditorUiAlias: `Test.Ui.${name}`,
			settings: { properties: settings.map((alias) => ({ alias, label: alias, propertyEditorUiAlias: 'Test' })) },
		},
	},
	{
		type: 'propertyEditorUi',
		alias: `Test.Ui.${name}`,
		name: `Test ${name} UI`,
		meta: { label: name, icon: 'icon-test', group: 'common', propertyEditorSchemaAlias: `Test.${name}` },
	},
];

const TEST_MANIFESTS: Array<UmbExtensionManifest> = [
	...dataTypeDetailRepositoryManifests,
	...(editor('From', ['oldName', 'shared']) as Array<UmbExtensionManifest>),
	...(editor('To', ['newName', 'shared']) as Array<UmbExtensionManifest>),
	...(editor('Other', ['shared']) as Array<UmbExtensionManifest>),
	...(editor('Failing', ['newName', 'shared']) as Array<UmbExtensionManifest>),
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Test.ConfigConverter.FromTo',
		name: 'Test From To Config Converter',
		api: UmbTestRenamingConverter,
		fromPropertyEditorSchemaAlias: 'Test.From',
		forPropertyEditorSchemaAlias: 'Test.To',
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Test.ConfigConverter.FromFailing',
		name: 'Test From Failing Config Converter',
		api: UmbTestFailingConverter,
		fromPropertyEditorSchemaAlias: 'Test.From',
		forPropertyEditorSchemaAlias: 'Test.Failing',
	},
];

describe('UmbDataTypeWorkspaceContext', () => {
	describe('changing the property editor', () => {
		let host: UmbTestDataTypeWorkspaceHostElement;
		let context: UmbDataTypeWorkspaceContext;

		before(() => {
			umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
		});

		after(() => {
			umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((x) => x.alias));
		});

		beforeEach(async () => {
			await useMockSet('default');
			umbDataTypeMockDb.create({
				id: DATA_TYPE_ID,
				parent: null,
				name: 'Config Converter Test',
				editorAlias: 'Test.From',
				editorUiAlias: 'Test.Ui.From',
				values: [
					{ alias: 'oldName', value: 42 },
					{ alias: 'shared', value: 'kept' },
				],
				hasChildren: false,
				isFolder: false,
				isDeletable: true,
				canIgnoreStartNodes: false,
				flags: [],
				noAccess: false,
			});

			host = new UmbTestDataTypeWorkspaceHostElement();
			document.body.appendChild(host);
			context = new UmbDataTypeWorkspaceContext(host);
			await context.load(DATA_TYPE_ID);
			await waitUntil(() => context.getPropertyEditorSchemaAlias() === 'Test.From', 'The data type did not load');
		});

		afterEach(() => {
			context.destroy();
			document.body.innerHTML = '';
		});

		const values = () => Object.fromEntries((context.getData()?.values ?? []).map((x) => [x.alias, x.value]));

		const changeEditor = async (name: string, expectedAlias: string) => {
			context.setPropertyEditorUiAlias(`Test.Ui.${name}`);
			await waitUntil(
				() => context.getPropertyEditorSchemaAlias() === `Test.${name}` && expectedAlias in values(),
				`The settings for Test.${name} were not applied`,
			);
		};

		it('converts the saved settings for the new property editor', async () => {
			await changeEditor('To', 'newName');

			expect(values()).to.deep.equal({ newName: 42, shared: 'kept' });
		});

		it('restores the saved settings when changed back to the saved property editor', async () => {
			await changeEditor('To', 'newName');
			await changeEditor('From', 'oldName');

			expect(values()).to.deep.equal({ oldName: 42, shared: 'kept' });
		});

		it('converts the saved settings after changing to another property editor first', async () => {
			await changeEditor('Other', 'shared');
			expect(values()).to.deep.equal({ shared: 'kept' });

			await changeEditor('To', 'newName');

			expect(values()).to.deep.equal({ newName: 42, shared: 'kept' });
		});

		it('converts settings changed before the property editor', async () => {
			await context.setPropertyValue('oldName', 7);

			await changeEditor('To', 'newName');

			expect(values()).to.deep.equal({ newName: 7, shared: 'kept' });
		});

		it('carries over settings by name when no converter is registered', async () => {
			await changeEditor('Other', 'shared');

			expect(values()).to.deep.equal({ shared: 'kept' });
		});

		it('carries over settings by name when the converter fails', async () => {
			await changeEditor('Failing', 'shared');

			expect(values()).to.deep.equal({ shared: 'kept' });
		});
	});
});
