import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockClipboardEntryValueModel } from 'src/packages/block/block/types';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants';
import type { UmbBlockGridValueModel } from '../../../types';
import { UmbBlockGridToGridBlockClipboardCopyTranslator } from './block-grid-to-grid-block-copy-translator.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockListToBlockClipboardCopyTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbBlockGridToGridBlockClipboardCopyTranslator;

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
		layout: blockGridPropertyValue.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS],
		settingsData: blockGridPropertyValue.settingsData,
		expose: blockGridPropertyValue.expose,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbBlockGridToGridBlockClipboardCopyTranslator(hostElement);
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
			const result = await copyTranslator.translate(blockGridPropertyValue);
			expect(result).to.deep.equal(blockClipboardEntryValue);
		});
	});
});
