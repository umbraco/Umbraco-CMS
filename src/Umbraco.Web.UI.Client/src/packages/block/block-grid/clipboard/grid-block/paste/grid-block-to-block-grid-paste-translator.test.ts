import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbBlockGridValueModel, UmbGridBlockClipboardEntryValueModel } from '../../../types';
import { UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator } from './grid-block-to-block-grid-paste-translator.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator;

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
		expose: [],
	};

	const gridBlockClipboardEntryValue: UmbGridBlockClipboardEntryValueModel = {
		contentData: blockGridPropertyValue.contentData,
		layout: blockGridPropertyValue.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS],
		settingsData: blockGridPropertyValue.settingsData,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator(hostElement);
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
		it('returns the block grid property value', async () => {
			const result = await copyTranslator.translate(gridBlockClipboardEntryValue);
			expect(result).to.deep.equal(blockGridPropertyValue);
		});
	});
});
