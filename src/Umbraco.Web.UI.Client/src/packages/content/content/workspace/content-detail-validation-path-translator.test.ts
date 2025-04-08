import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentDetailValidationPathTranslator } from './content-detail-validation-path-translator.js';
import {
	UmbDataPathPropertyValueQuery,
	UmbValidationPathTranslationController,
} from '@umbraco-cms/backoffice/validation';
import type { UmbDocumentDetailModel } from '@umbraco-cms/backoffice/document';
import type { UmbValidationMessage } from '../../../core/validation/context/validation-messages.manager.js';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbValidationPropertyPathTranslationController', () => {
	describe('Without translators', () => {
		let host: UmbTestControllerHostElement;
		let ctrl!: UmbValidationPathTranslationController;
		let documentData: UmbDocumentDetailModel = {
			documentType: {
				unique: 'test',
				collection: null,
			},
			entityType: 'document',
			unique: 'test',
			isTrashed: false,
			template: null,
			urls: [],
			values: [
				{
					alias: 'headline',
					editorAlias: 'Umbraco.TestProperty',
					value: 'value1',
					culture: null,
					segment: null,
				},
			],
			variants: [],
		};

		beforeEach(async () => {
			host = new UmbTestControllerHostElement();
			ctrl = new UmbValidationPathTranslationController(host, {
				translationData: documentData,
				pathTranslators: [UmbContentDetailValidationPathTranslator],
			});
		});
		afterEach(async () => {
			host.destroy();
		});

		it('translates excisting value', async () => {
			let messages: Array<UmbValidationMessage> = [
				{
					type: 'client',
					key: '1',
					path: '$.values[0].value',
					body: 'test message',
				},
			];

			messages = await ctrl.translate(messages);

			expect(messages[0].path).to.be.equal(`$.values[${UmbDataPathPropertyValueQuery(documentData.values[0])}].value`);
		});

		it('still works with messages for non-existing aliases', async () => {
			let messages: Array<UmbValidationMessage> = [
				{
					type: 'client',
					key: '07798e58-186a-4662-93e7-ec06f3074d43',
					path: "$.values[?(@.alias == 'notExisting' && @.culture == null && @.segment == null)].value",
					body: 'Du skal skrive en notExisting',
				},
			];

			messages = await ctrl.translate(messages);

			expect(messages[0].path).to.be.equal(
				`$.values[${UmbDataPathPropertyValueQuery({ ...documentData.values[0], alias: 'notExisting' })}].value`,
			);
		});

		it('still works with messages for non-existing cultures', async () => {
			let messages: Array<UmbValidationMessage> = [
				{
					type: 'client',
					key: '07798e58-186a-4662-93e7-ec06f3074d43',
					path: "$.values[?(@.alias == 'headline' && @.culture == 'da' && @.segment == null)].value",
					body: 'Du skal skrive en headline',
				},
				{
					type: 'client',
					key: '300a5fe0-776f-4bcf-ac68-b4073900c6ef',
					path: "$.values[?(@.alias == 'headline' && @.culture == 'en-US' && @.segment == null)].value",
					body: 'Du skal skrive en headline',
				},
			];

			messages = await ctrl.translate(messages);

			expect(messages.length).to.be.equal(2);
		});
	});
});
