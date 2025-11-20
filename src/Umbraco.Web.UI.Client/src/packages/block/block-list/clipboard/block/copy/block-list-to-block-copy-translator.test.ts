import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockListValueModel } from '../../../types.js';
import { UmbBlockListToBlockClipboardCopyPropertyValueTranslator } from './block-list-to-block-copy-translator';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockListToBlockClipboardCopyPropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbBlockListToBlockClipboardCopyPropertyValueTranslator;

	const blockListPropertyValue: UmbBlockListValueModel = {
		contentData: [
			{
				key: 'contentKey',
				contentTypeKey: 'contentTypeKey',
				values: [
					{
						culture: null,
						segment: null,
						alias: 'headline',
						editorAlias: 'Umbraco.TextBox',
						value: 'Headline value',
						entityType: '',
					},
				],
			},
		],
		layout: {
			[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: [
				{
					contentKey: 'contentKey',
					settingsKey: null,
				},
			],
		},
		settingsData: [],
		expose: [
			{
				contentKey: 'contentKey',
				culture: null,
				segment: null,
			},
		],
	};

	const blockClipboardEntryValue: UmbBlockClipboardEntryValueModel = {
		contentData: blockListPropertyValue.contentData,
		layout: [
			{
				contentKey: 'contentKey',
				settingsKey: null,
			},
		],
		settingsData: blockListPropertyValue.settingsData,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbBlockListToBlockClipboardCopyPropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a translate method', () => {
				expect(copyTranslator).to.have.property('translate').that.is.a('function');
			});
		});
	});

	describe('translate', () => {
		it('returns the block clipboard entry value', async () => {
			const result = await copyTranslator.translate(blockListPropertyValue);
			expect(result).to.deep.equal(blockClipboardEntryValue);
		});
	});
});
