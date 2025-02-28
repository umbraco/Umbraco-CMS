import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbValidationPathTranslator } from '../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbValidationTranslatorController } from './validation-path-translation.manager.js';
import type { UmbValidationMessage } from '../../context/validation-messages.manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbScopeMapperForJsonPaths } from '../../utils/scope-mapper-json-paths.function.js';
import { umbQueryMapperForJsonPaths } from '../../utils/query-mapper-json-paths.function.js';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbValidationTranslatorController', () => {
	describe('Without translators', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationTranslatorController;
		let translationData = {
			propertyStr: 'value',
			propertyArray: [0, 1, 2, 3],
			propertyObj: {
				propStr: 'value',
			},
		};

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationTranslatorController(host, { translationData, pathTranslators: [] });
		});
		afterEach(async () => {
			host.destroy();
		});

		it('returns value', async () => {
			const msgs: Array<UmbValidationMessage> = [
				{
					key: '1',
					type: 'client',
					path: '$.propertyStr',
					body: 'test',
				},
			];

			const result = await ctrl.translate(msgs);

			expect(result[0].key).to.be.equal('1');
			expect(result[0].path).to.be.equal('$.propertyStr');
		});
	});

	describe('Using a translator', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationTranslatorController;
		let translationData = {
			propArray: [
				{
					key: '1',
					name: 'array1',
					inner: [
						{
							key: '1a',
							name: 'inner1a',
						},
						{
							key: '1b',
							name: 'inner1b',
						},
					],
				},
				{
					key: '2',
					name: 'array2',
					inner: [
						{
							key: '2a',
							name: 'inner2a',
						},
						{
							key: '2b',
							name: 'inner2b',
						},
					],
				},
			],
		};

		class Translator extends UmbControllerBase implements UmbValidationPathTranslator<typeof translationData> {
			async translate(paths: Array<string>, data: typeof translationData): Promise<Array<string>> {
				// Method to filter and cut the initial path, returning an array:
				return umbScopeMapperForJsonPaths(paths, '$.propArray', async (innerPaths) => {
					return await umbQueryMapperForJsonPaths(
						innerPaths,
						data.propArray,
						(entry: (typeof translationData.propArray)[0]) => {
							return `?(@.key == '${entry.key}')`;
						},
					);
				});
			}
		}

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationTranslatorController(host, { translationData, pathTranslators: [Translator] });
		});
		afterEach(async () => {
			host.destroy();
		});

		it('returns value', async () => {
			const msgs: Array<UmbValidationMessage> = [
				{
					key: '1',
					type: 'client',
					path: '$.propArray[0].innerValue',
					body: 'test',
				},
			];

			const result = await ctrl.translate(msgs);

			expect(result[0].key).to.be.equal('1');
			expect(result[0].path).to.be.equal(`$.propArray[?(@.key == '1')].innerValue`);
		});
	});
});
