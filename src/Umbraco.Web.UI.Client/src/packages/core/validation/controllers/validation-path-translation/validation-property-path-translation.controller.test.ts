import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbValidationPropertyPathTranslationController } from './validation-property-path-translation.controller.js';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { UmbDataPathPropertyValueQuery } from '../../utils/data-path-property-value-query.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPropertyValidationPathTranslator } from './types.js';
import { umbScopeMapperForJsonPaths } from '../../utils/scope-mapper-json-paths.function.js';
import { umbQueryMapperForJsonPaths } from '../../utils/query-mapper-json-paths.function.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbValidationPropertyPathTranslationController', () => {
	describe('Without translators', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationPropertyPathTranslationController;
		let propertiesData: Array<UmbPropertyValueDataPotentiallyWithEditorAlias> = [
			{
				alias: 'test-alias',
				editorAlias: 'Umbraco.TestProperty',
				value: 'value1',
			},
		];

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationPropertyPathTranslationController(host);
		});
		afterEach(async () => {
			host.destroy();
		});

		it('returns value', async () => {
			const paths: Array<string> = ['$[0].value'];

			const result = await ctrl.translateProperties(paths, propertiesData, UmbDataPathPropertyValueQuery);

			expect(result[0]).to.be.equal(`$[${UmbDataPathPropertyValueQuery(propertiesData[0])}].value`);
		});
	});

	describe('capital Value Without translators', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationPropertyPathTranslationController;
		let propertiesData: Array<UmbPropertyValueDataPotentiallyWithEditorAlias> = [
			{
				alias: 'test-alias',
				editorAlias: 'Umbraco.TestProperty',
				Value: 'Value1',
			} as any,
		];

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationPropertyPathTranslationController(host);
		});
		afterEach(async () => {
			host.destroy();
		});

		it('returns Value', async () => {
			const paths: Array<string> = ['$[0].Value'];

			const result = await ctrl.translateProperties(paths, propertiesData, UmbDataPathPropertyValueQuery);

			expect(result[0]).to.be.equal(`$[${UmbDataPathPropertyValueQuery(propertiesData[0])}].Value`);
		});
	});

	type ComplexPropertyTypeValue = {
		inner: Array<{
			key: string;
			name: string;
		}>;
	};
	class ComplexPropertyPathTranslator
		extends UmbControllerBase
		implements UmbPropertyValidationPathTranslator<ComplexPropertyTypeValue>
	{
		async translate(
			paths: Array<string>,
			data: UmbPropertyValueDataPotentiallyWithEditorAlias<ComplexPropertyTypeValue>,
		): Promise<Array<string>> {
			// Method to filter and cut the initial path, returning an array:
			return umbScopeMapperForJsonPaths(paths, '$.value.inner', async (innerPaths) => {
				if (!data.value) {
					return innerPaths;
				}
				return await umbQueryMapperForJsonPaths(innerPaths, data.value.inner, (entry) => {
					return `?(@.key == '${entry.key}')`;
				});
			});
		}
	}

	const ComplexPropertyPathTranslatorManifest = {
		alias: 'Umbraco.TestProperty.ComplexPathTranslator',
		name: 'Complex Path Translator',
		type: 'propertyValidationPathTranslator',
		forEditorAlias: 'Umbraco.TestProperty',
		api: ComplexPropertyPathTranslator,
	};

	describe('With translators', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationPropertyPathTranslationController;
		let propertiesData: Array<UmbPropertyValueDataPotentiallyWithEditorAlias<ComplexPropertyTypeValue>> = [
			{
				alias: 'test-alias',
				editorAlias: 'Umbraco.TestProperty',
				value: {
					inner: [
						{
							key: 'key1',
							name: 'inner1',
						},
						{
							key: 'key2',
							name: 'inner2',
						},
					],
				},
			},
		];

		beforeEach(async () => {
			umbExtensionsRegistry.register(ComplexPropertyPathTranslatorManifest);
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationPropertyPathTranslationController(host);
		});
		afterEach(async () => {
			host.destroy();
			umbExtensionsRegistry.unregister(ComplexPropertyPathTranslatorManifest.alias);
		});

		it('returns value', async () => {
			const paths: Array<string> = ['$[0].value.inner[1].name'];

			const result = await ctrl.translateProperties(paths, propertiesData, UmbDataPathPropertyValueQuery);

			expect(result[0]).to.be.equal(
				`$[${UmbDataPathPropertyValueQuery(propertiesData[0])}].value.inner[?(@.key == 'key2')].name`,
			);
		});
	});

	describe('With missing translator', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationPropertyPathTranslationController;
		let propertiesData: Array<UmbPropertyValueDataPotentiallyWithEditorAlias<ComplexPropertyTypeValue>> = [
			{
				alias: 'test-alias',
				editorAlias: 'Umbraco.TestProperty',
				value: {
					inner: [
						{
							key: 'key1',
							name: 'inner1',
						},
						{
							key: 'key2',
							name: 'inner2',
						},
					],
				},
			},
		];

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationPropertyPathTranslationController(host);
		});
		afterEach(async () => {
			host.destroy();
		});

		it('returns value', async () => {
			const paths: Array<string> = ['$[0].value.inner[1].name'];

			const result = await ctrl.translateProperties(paths, propertiesData, UmbDataPathPropertyValueQuery);

			expect(result[0]).to.be.equal(`$[${UmbDataPathPropertyValueQuery(propertiesData[0])}].value.inner[1].name`);
		});
	});
});
