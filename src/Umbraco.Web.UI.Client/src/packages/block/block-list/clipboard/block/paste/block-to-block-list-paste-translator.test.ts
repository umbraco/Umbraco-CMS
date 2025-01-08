import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockListValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from 'src/packages/block/block/types';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import { UmbBlockToBlockListClipboardPasteTranslator } from './block-to-block-list-paste-translator';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockToBlockListClipboardPasteTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let pasteTranslator: UmbBlockToBlockListClipboardPasteTranslator;

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
			},
		],
		settingsData: blockListPropertyValue.settingsData,
		expose: blockListPropertyValue.expose,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		pasteTranslator = new UmbBlockToBlockListClipboardPasteTranslator(hostElement);
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
});
