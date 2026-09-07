import { UmbPropertyEditorUiRteElementBase } from './rte-base.element.js';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorRteValueType } from '../types.js';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';

@customElement('umb-test-rte-base')
class UmbTestRteBaseElement extends UmbPropertyEditorUiRteElementBase {
	public callFilterUnusedBlocksFromMarkup(markup: string) {
		this._filterUnusedBlocksFromMarkup(markup);
	}

	// eslint-disable-next-line @typescript-eslint/no-deprecated
	public callFilterUnusedBlocks(usedLayoutKeys: Array<string | null>) {
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		this._filterUnusedBlocks(usedLayoutKeys);
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-test-rte-base': UmbTestRteBaseElement;
	}
}

function makeValue(layouts: Array<UmbBlockRteLayoutModel>, contents: Array<UmbBlockDataModel>): UmbPropertyEditorRteValueType {
	return {
		markup: '',
		blocks: {
			layout: { [UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
			contentData: contents,
			settingsData: [],
			expose: [],
		},
	};
}

const LAYOUT_A: UmbBlockRteLayoutModel = { key: 'layout-a', contentKey: 'content-a' };
const CONTENT_A: UmbBlockDataModel = { key: 'content-a', contentTypeKey: 'content-type', values: [] };
const LAYOUT_B: UmbBlockRteLayoutModel = { key: 'layout-b', contentKey: 'content-b' };
const CONTENT_B: UmbBlockDataModel = { key: 'content-b', contentTypeKey: 'content-type', values: [] };

describe('UmbPropertyEditorUiRteElementBase', () => {
	let element: UmbTestRteBaseElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-rte-base></umb-test-rte-base>`);
		// The manager -> value sync observer is wired up via an asynchronously resolved context; let it settle.
		await aTimeout(0);
	});

	describe('_filterUnusedBlocksFromMarkup', () => {
		it('keeps only blocks referenced by data-key in the given markup', () => {
			element.value = makeValue([LAYOUT_A, LAYOUT_B], [CONTENT_A, CONTENT_B]);

			element.callFilterUnusedBlocksFromMarkup('<umb-rte-block data-key="layout-a" data-content-key="content-a"></umb-rte-block>');

			const layouts = element.value?.blocks?.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS];
			expect(layouts).to.deep.equal([LAYOUT_A]);
			expect(element.value?.blocks?.contentData).to.deep.equal([CONTENT_A]);
		});

		it('falls back to data-content-key for legacy layouts with no separate key', () => {
			const legacyLayout: UmbBlockRteLayoutModel = { key: 'content-c', contentKey: 'content-c' };
			const legacyContent: UmbBlockDataModel = { key: 'content-c', contentTypeKey: 'content-type', values: [] };
			element.value = makeValue([legacyLayout], [legacyContent]);

			element.callFilterUnusedBlocksFromMarkup('<umb-rte-block data-content-key="content-c"></umb-rte-block>');

			const layouts = element.value?.blocks?.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS];
			expect(layouts).to.deep.equal([legacyLayout]);
		});

		it('removes a block entirely absent from the markup', () => {
			element.value = makeValue([LAYOUT_A], [CONTENT_A]);

			element.callFilterUnusedBlocksFromMarkup('<p>No blocks here.</p>');

			const layouts = element.value?.blocks?.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS];
			expect(layouts).to.deep.equal([]);
			expect(element.value?.blocks?.contentData).to.deep.equal([]);
		});
	});

	// The deprecated overload must keep working for existing RTE implementations built against it.
	describe('_filterUnusedBlocks (deprecated)', () => {
		it('keeps only blocks referenced by the given layout keys', () => {
			element.value = makeValue([LAYOUT_A, LAYOUT_B], [CONTENT_A, CONTENT_B]);

			element.callFilterUnusedBlocks(['layout-a']);

			const layouts = element.value?.blocks?.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS];
			expect(layouts).to.deep.equal([LAYOUT_A]);
			expect(element.value?.blocks?.contentData).to.deep.equal([CONTENT_A]);
		});
	});
});
