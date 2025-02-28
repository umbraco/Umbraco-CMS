import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbValidationPropertyPathTranslatorController } from './validation-property-path-translation.manager.js';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { UmbDataPathPropertyValueQuery } from '../../utils/data-path-property-value-query.function.js';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbValidationPropertyPathTranslatorController', () => {
	describe('Without translators', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationPropertyPathTranslatorController;
		let propertiesData: Array<UmbPropertyValueDataPotentiallyWithEditorAlias> = [
			{
				alias: 'test-alias',
				editorAlias: 'Umbraco.TestProperty',
				value: 'value1',
			},
		];

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationPropertyPathTranslatorController(host);
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
});
