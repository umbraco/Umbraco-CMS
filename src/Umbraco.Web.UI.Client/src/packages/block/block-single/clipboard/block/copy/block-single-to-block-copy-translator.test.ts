import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockSingleValueModel } from '../../../types.js';
import { UmbBlockSingleToBlockClipboardCopyPropertyValueTranslator } from './block-single-to-block-copy-translator.js';
import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockSingleToBlockClipboardCopyPropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbBlockSingleToBlockClipboardCopyPropertyValueTranslator;

	const blockSinglePropertyValue: UmbBlockSingleValueModel = {
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
			[UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS]: [
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
		contentData: blockSinglePropertyValue.contentData,
		layout: [
			{
				contentKey: 'contentKey',
				settingsKey: null,
			},
		],
		settingsData: blockSinglePropertyValue.settingsData,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbBlockSingleToBlockClipboardCopyPropertyValueTranslator(hostElement);
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
			const result = await copyTranslator.translate(blockSinglePropertyValue);
			expect(result).to.deep.equal(blockClipboardEntryValue);
		});
	});
});
