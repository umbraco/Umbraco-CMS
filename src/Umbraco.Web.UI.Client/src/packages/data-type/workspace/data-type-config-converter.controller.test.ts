import { UmbDataTypeConfigConverterController } from './data-type-config-converter.controller.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyEditorConfigConverter,
	UmbPropertyEditorConfig,
	UmbPropertyEditorConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('test-data-type-config-converter-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestRenamingConverter extends UmbControllerBase implements UmbPropertyEditorConfigConverter {
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
		return [
			{ alias: 'newName', value: config.find((x) => x.alias === 'oldName')?.value },
			{ alias: 'shared', value: 'from the renaming converter' },
		];
	}
}

class UmbTestHeavierConverter extends UmbControllerBase implements UmbPropertyEditorConfigConverter {
	async convert(): Promise<UmbPropertyEditorConfig> {
		return [{ alias: 'shared', value: 'from the heavier converter' }];
	}
}

class UmbTestFailingConverter extends UmbControllerBase implements UmbPropertyEditorConfigConverter {
	convert(): UmbPropertyEditorConfig {
		throw new Error('Test converter failure');
	}
}

const fromSchemaAlias = 'Test.From';
const toSchemaAlias = 'Test.To';

const manifests: Array<ManifestPropertyEditorConfigConverter> = [
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Test.ConfigConverter.Renaming',
		name: 'Test Renaming Config Converter',
		api: UmbTestRenamingConverter,
		fromPropertyEditorSchemaAlias: fromSchemaAlias,
		forPropertyEditorSchemaAlias: toSchemaAlias,
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Test.ConfigConverter.Heavier',
		name: 'Test Heavier Config Converter',
		api: UmbTestHeavierConverter,
		weight: 100,
		fromPropertyEditorSchemaAlias: fromSchemaAlias,
		forPropertyEditorSchemaAlias: toSchemaAlias,
	},
];

describe('UmbDataTypeConfigConverterController', () => {
	let controller: UmbDataTypeConfigConverterController;

	beforeEach(() => {
		const host = new UmbTestControllerHostElement();
		document.body.innerHTML = '';
		document.body.appendChild(host);
		controller = new UmbDataTypeConfigConverterController(host);
		umbExtensionsRegistry.registerMany(manifests);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(manifests.map((x) => x.alias));
	});

	describe('hasConverter', () => {
		it('is true for a pair of property editor schemas with a registered converter', () => {
			expect(controller.hasConverter(fromSchemaAlias, toSchemaAlias)).to.be.true;
		});

		it('is false for the reverse direction', () => {
			expect(controller.hasConverter(toSchemaAlias, fromSchemaAlias)).to.be.false;
		});
	});

	describe('convert', () => {
		it('combines the settings returned by every converter, preferring the converter of higher weight', async () => {
			const result = await controller.convert(fromSchemaAlias, toSchemaAlias, [{ alias: 'oldName', value: 42 }]);

			expect(result).to.deep.equal([
				{ alias: 'shared', value: 'from the heavier converter' },
				{ alias: 'newName', value: 42 },
			]);
		});

		it('skips a converter that fails, so the others still apply', async () => {
			const failing: ManifestPropertyEditorConfigConverter = {
				type: 'propertyEditorConfigConverter',
				alias: 'Test.ConfigConverter.Failing',
				name: 'Test Failing Config Converter',
				api: UmbTestFailingConverter,
				weight: 1000,
				fromPropertyEditorSchemaAlias: fromSchemaAlias,
				forPropertyEditorSchemaAlias: toSchemaAlias,
			};
			umbExtensionsRegistry.register(failing);

			try {
				const result = await controller.convert(fromSchemaAlias, toSchemaAlias, [{ alias: 'oldName', value: 42 }]);

				expect(result).to.deep.equal([
					{ alias: 'shared', value: 'from the heavier converter' },
					{ alias: 'newName', value: 42 },
				]);
			} finally {
				umbExtensionsRegistry.unregister(failing.alias);
			}
		});

		it('converts nothing when no converter is registered for the property editor schemas', async () => {
			const result = await controller.convert(toSchemaAlias, fromSchemaAlias, [{ alias: 'oldName', value: 42 }]);

			expect(result).to.have.lengthOf(0);
		});
	});
});
