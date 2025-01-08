import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockListValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from 'src/packages/block/block/types';
import { UmbBlockListToBlockClipboardCopyTranslator } from './block-list-to-block-copy-translator';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockListToBlockClipboardCopyTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbBlockListToBlockClipboardCopyTranslator;

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
		copyTranslator = new UmbBlockListToBlockClipboardCopyTranslator(hostElement);
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
		it('returns the block clipboard entry value model', async () => {
			const result = await copyTranslator.translate(blockListPropertyValue);
			expect(result).to.deep.equal(blockClipboardEntryValue);
		});
	});
});
