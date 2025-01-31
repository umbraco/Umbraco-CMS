import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import { UmbBlockGridToBlockClipboardCopyPropertyValueTranslator } from './block-grid-to-block-copy-translator.js';
import type { UmbBlockGridValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockListToBlockClipboardCopyPropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbBlockGridToBlockClipboardCopyPropertyValueTranslator;

	const blockGridPropertyValue: UmbBlockGridValueModel = {
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
			[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: [
				{
					columnSpan: 12,
					rowSpan: 1,
					areas: [],
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
		contentData: blockGridPropertyValue.contentData,
		layout: [
			{
				contentKey: 'contentKey',
				settingsKey: null,
			},
		],
		settingsData: blockGridPropertyValue.settingsData,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbBlockGridToBlockClipboardCopyPropertyValueTranslator(hostElement);
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
			const result = await copyTranslator.translate(blockGridPropertyValue);
			expect(result).to.deep.equal(blockClipboardEntryValue);
		});
	});
});
