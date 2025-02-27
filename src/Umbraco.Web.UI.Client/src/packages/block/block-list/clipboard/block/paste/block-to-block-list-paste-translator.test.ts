import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockListValueModel } from '../../../types.js';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import { UmbBlockToBlockListClipboardPastePropertyValueTranslator } from './block-to-block-list-paste-translator';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockToBlockListClipboardPastePropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let pasteTranslator: UmbBlockToBlockListClipboardPastePropertyValueTranslator;

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
		expose: [],
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

	const config: Array<{ alias: string; value: [{ contentElementTypeKey: string }] }> = [
		{
			alias: 'blocks',
			value: [
				{
					contentElementTypeKey: 'contentTypeKey',
				},
			],
		},
	];

	const config2: Array<{ alias: string; value: [{ contentElementTypeKey: string }] }> = [
		{
			alias: 'blocks',
			value: [
				{
					contentElementTypeKey: 'contentTypeKey2',
				},
			],
		},
	];

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		pasteTranslator = new UmbBlockToBlockListClipboardPastePropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a translate method', () => {
				expect(pasteTranslator).to.have.property('translate').that.is.a('function');
			});
		});
	});

	describe('translate', () => {
		it('return the block list property value', async () => {
			const result = await pasteTranslator.translate(blockClipboardEntryValue);
			expect(result).to.deep.equal(blockListPropertyValue);
		});
	});

	describe('isCompatibleValue', () => {
		it('should return true if the content types are allowed', async () => {
			const result = await pasteTranslator.isCompatibleValue(blockClipboardEntryValue, config);
			expect(result).to.be.true;
		});

		it('should return false if the content types are not allowed', async () => {
			const result = await pasteTranslator.isCompatibleValue(blockClipboardEntryValue, config2);
			expect(result).to.be.false;
		});
	});
});
