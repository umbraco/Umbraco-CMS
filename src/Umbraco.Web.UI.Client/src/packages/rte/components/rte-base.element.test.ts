import { UmbPropertyEditorUiRteElementBase } from './rte-base.element.js';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { UmbPropertyEditorRteValueType } from '../types.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteManagerContext } from '@umbraco-cms/backoffice/block-rte';
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
	// Asserted against directly: _filterUnusedBlocksByLayoutKeys' contract is mutating manager state.
	// element.value only re-syncs from that state via a property-context-dependent observer, which this
	// fixture (no real UMB_PROPERTY_CONTEXT provider) never wires up.
	let managerContext: UmbBlockRteManagerContext;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-rte-base></umb-test-rte-base>`);
		managerContext = (await element.getContext(UMB_BLOCK_RTE_MANAGER_CONTEXT))!;
	});

	describe('_filterUnusedBlocksFromMarkup', () => {
		it('keeps only blocks referenced by data-key in the given markup', () => {
			element.value = makeValue([LAYOUT_A, LAYOUT_B], [CONTENT_A, CONTENT_B]);

			element.callFilterUnusedBlocksFromMarkup('<umb-rte-block data-key="layout-a" data-content-key="content-a"></umb-rte-block>');

			expect(managerContext.getLayouts()).to.deep.equal([LAYOUT_A]);
			expect(managerContext.getContents()).to.deep.equal([CONTENT_A]);
		});

		it('falls back to data-content-key for legacy layouts with no separate key', () => {
			const legacyLayout: UmbBlockRteLayoutModel = { key: 'content-c', contentKey: 'content-c' };
			const legacyContent: UmbBlockDataModel = { key: 'content-c', contentTypeKey: 'content-type', values: [] };
			element.value = makeValue([legacyLayout], [legacyContent]);

			element.callFilterUnusedBlocksFromMarkup('<umb-rte-block data-content-key="content-c"></umb-rte-block>');

			expect(managerContext.getLayouts()).to.deep.equal([legacyLayout]);
		});

		it('removes a block entirely absent from the markup', () => {
			element.value = makeValue([LAYOUT_A], [CONTENT_A]);

			element.callFilterUnusedBlocksFromMarkup('<p>No blocks here.</p>');

			expect(managerContext.getLayouts()).to.deep.equal([]);
			expect(managerContext.getContents()).to.deep.equal([]);
		});

		// Removing content/settings emits synchronously; if a still-present layout's content stays
		// resolvable while that happens, #updateBlocks (block.tiptap-api.ts) re-inserts the node before
		// the layout removal catches up, orphaning it. Layouts must therefore be removed first.
		it('removes the layout before removing its content', () => {
			element.value = makeValue([LAYOUT_A], [CONTENT_A]);

			const removalOrder: Array<'layouts' | 'contents'> = [];
			managerContext.layouts.subscribe((layouts) => {
				if (!layouts.some((x) => x.key === LAYOUT_A.key)) removalOrder.push('layouts');
			});
			managerContext.contents.subscribe((contents) => {
				if (!contents.some((x) => x.key === CONTENT_A.key)) removalOrder.push('contents');
			});

			element.callFilterUnusedBlocksFromMarkup('<p>No blocks here.</p>');

			expect(removalOrder).to.deep.equal(['layouts', 'contents']);
		});
	});

	// The deprecated overload must keep working for existing RTE implementations built against it.
	describe('_filterUnusedBlocks (deprecated)', () => {
		it('keeps only blocks referenced by the given layout keys', () => {
			element.value = makeValue([LAYOUT_A, LAYOUT_B], [CONTENT_A, CONTENT_B]);

			element.callFilterUnusedBlocks(['layout-a']);

			expect(managerContext.getLayouts()).to.deep.equal([LAYOUT_A]);
			expect(managerContext.getContents()).to.deep.equal([CONTENT_A]);
		});
	});
});
