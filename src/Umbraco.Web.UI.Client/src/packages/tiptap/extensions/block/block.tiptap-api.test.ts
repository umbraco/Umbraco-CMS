import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { umbRteBlock, umbRteBlockInline } from './block.tiptap-extension.js';
import UmbTiptapBlockElementApi from './block.tiptap-api.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import { UMB_BLOCK_RTE_DATA_LAYOUT_KEY } from '@umbraco-cms/backoffice/rte';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';

@customElement('umb-test-tiptap-block-api-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

const LOCAL_LAYOUT_KEY = 'local-layout';
const LOCAL_CONTENT_KEY = 'local-content';
const EXTERNAL_LAYOUT_KEY = 'external-layout';
const EXTERNAL_CONTENT_KEY = 'external-content';
const CONTENT_TYPE_KEY = 'content-type';

function getLayoutKeysInDoc(editor: Editor): Array<string> {
	const keys: Array<string> = [];
	editor.state.doc.descendants((node) => {
		const key = node.attrs[UMB_BLOCK_RTE_DATA_LAYOUT_KEY];
		if (key) keys.push(key);
		return true;
	});
	return keys;
}

// Regression coverage for the orphan-block bug fixed alongside the layout-key rework: a library-element
// (external) block's content is never purged from the manager's externalContentValues state, so if its
// layout entry is still present when #updateBlocks re-observes, it looks like a block that needs
// (re-)inserting — even though the editor already had its node removed by the delete flow. Reproduced
// here directly against UmbTiptapBlockElementApi with a fake manager context, since the real
// UmbBlockRteManagerContext pulls in a live element repository that isn't needed to exercise this path.
describe('UmbTiptapBlockElementApi', () => {
	let host: UmbElement;
	let layoutsState: UmbArrayState<UmbBlockRteLayoutModel>;
	let allContentsState: UmbArrayState<UmbBlockDataModel>;
	let pendingDeletionsState: UmbArrayState<string>;
	let editor: Editor;

	const setUp = async () => {
		host = await fixture(html`<umb-test-tiptap-block-api-host></umb-test-tiptap-block-api-host>`);

		layoutsState = new UmbArrayState<UmbBlockRteLayoutModel>(
			[
				{ key: LOCAL_LAYOUT_KEY, contentKey: LOCAL_CONTENT_KEY },
				{ key: EXTERNAL_LAYOUT_KEY, contentKey: EXTERNAL_CONTENT_KEY, isExternalContent: true },
			],
			(x) => x.key,
		);
		allContentsState = new UmbArrayState<UmbBlockDataModel>(
			[
				{ key: LOCAL_CONTENT_KEY, contentTypeKey: CONTENT_TYPE_KEY, values: [] },
				{ key: EXTERNAL_CONTENT_KEY, contentTypeKey: CONTENT_TYPE_KEY, values: [] },
			],
			(x) => x.key,
		);
		pendingDeletionsState = new UmbArrayState<string>([], (x) => x);

		host.provideContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, {
			getHostElement: () => host,
			layouts: layoutsState.asObservable(),
			allContents: allContentsState.asObservable(),
			pendingDeletions: pendingDeletionsState.asObservable(),
			getContentTypeKeyOfContentKey: (contentKey: string) =>
				allContentsState.getValue().find((x) => x.key === contentKey)?.contentTypeKey,
			getBlockTypeOf: () => ({ displayInline: false }),
			clearPendingDeletion: () => undefined,
		} as never);

		editor = new Editor({
			element: document.createElement('div'),
			extensions: [Document, Paragraph, Text, umbRteBlock, umbRteBlockInline],
			content:
				`<umb-rte-block data-key="${LOCAL_LAYOUT_KEY}" data-content-key="${LOCAL_CONTENT_KEY}"></umb-rte-block>` +
				`<umb-rte-block data-key="${EXTERNAL_LAYOUT_KEY}" data-content-key="${EXTERNAL_CONTENT_KEY}"></umb-rte-block>`,
		});

		const api = new UmbTiptapBlockElementApi(host);
		api.setEditor(editor);
		// consumeContext resolves asynchronously; let the initial subscription settle.
		await aTimeout(0);

		// Baseline: both blocks already in the doc, so the initial #updateBlocks pass inserts nothing new.
		expect(getLayoutKeysInDoc(editor)).to.have.members([LOCAL_LAYOUT_KEY, EXTERNAL_LAYOUT_KEY]);

		// Simulate the delete flow already having removed both nodes from the editor DOM, before the
		// manager state has been cleaned up (mirrors _filterUnusedBlocks running after the markup change).
		editor.commands.setContent('<p></p>');
		expect(getLayoutKeysInDoc(editor)).to.deep.equal([]);
	};

	afterEach(() => {
		editor?.destroy();
	});

	it('re-inserts an orphan external block when content is removed before its layout (buggy order)', async () => {
		await setUp();

		// Old, buggy order: content first (only the local content is actually removed — external content
		// lives outside of what removeManyContent touches), layouts still contain both entries.
		allContentsState.setValue([{ key: EXTERNAL_CONTENT_KEY, contentTypeKey: CONTENT_TYPE_KEY, values: [] }]);

		// #updateBlocks sees the external layout still present and its content still resolvable, and
		// re-inserts it into the (now empty) editor doc.
		expect(getLayoutKeysInDoc(editor)).to.deep.equal([EXTERNAL_LAYOUT_KEY]);

		layoutsState.setValue([]);

		// The layout is now gone, but the re-inserted node is left behind with nothing backing it.
		expect(getLayoutKeysInDoc(editor)).to.deep.equal([EXTERNAL_LAYOUT_KEY]);
	});

	it('does not re-insert the external block when layouts are removed before content (fixed order)', async () => {
		await setUp();

		// Fixed order: layouts first.
		layoutsState.setValue([]);
		allContentsState.setValue([{ key: EXTERNAL_CONTENT_KEY, contentTypeKey: CONTENT_TYPE_KEY, values: [] }]);

		// By the time content emits, the external layout is already gone, so there's nothing to re-insert.
		expect(getLayoutKeysInDoc(editor)).to.deep.equal([]);
	});
});
