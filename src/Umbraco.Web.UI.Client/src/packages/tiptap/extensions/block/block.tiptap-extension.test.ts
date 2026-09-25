import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY, UMB_BLOCK_RTE_DATA_LAYOUT_KEY } from '@umbraco-cms/backoffice/rte';
import { umbRteBlock, umbRteBlockInline } from './block.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('block.tiptap-extension', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			extensions: [Document, Paragraph, Text, umbRteBlock, umbRteBlockInline],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	// #updateBlocks (block.tiptap-api.ts) discovers existing blocks by walking `editor.state.doc`
	// rather than querying `editor.view.dom`, since `view` is not available before the editor mounts.
	// This asserts that walk finds the same content keys the DOM query used to.
	it('finds block and inline-block content keys by walking the document', () => {
		editor.commands.setContent(
			'<umb-rte-block data-content-key="block-1"></umb-rte-block>' +
				'<p>Hello <umb-rte-block-inline data-content-key="inline-1"></umb-rte-block-inline> world.</p>',
		);

		const foundKeys: Array<string> = [];
		editor.state.doc.descendants((node) => {
			const contentKey = node.attrs[UMB_BLOCK_RTE_DATA_CONTENT_KEY];
			if (contentKey) foundKeys.push(contentKey);
			return true;
		});

		expect(foundKeys).to.deep.equal(['block-1', 'inline-1']);
	});

	// #updateBlocks (block.tiptap-api.ts) now discovers existing blocks by layout key, not content key,
	// since layout entries are the real identity for RTE blocks. This asserts the walk finds layout keys,
	// including the legacy-markup fallback in getBlockAttrs where a missing data-key falls back to the content key.
	it('finds block and inline-block layout keys by walking the document', () => {
		editor.commands.setContent(
			'<umb-rte-block data-key="layout-1" data-content-key="block-1"></umb-rte-block>' +
				'<p><umb-rte-block-inline data-content-key="inline-1"></umb-rte-block-inline></p>',
		);

		const foundKeys: Array<string> = [];
		editor.state.doc.descendants((node) => {
			const layoutKey = node.attrs[UMB_BLOCK_RTE_DATA_LAYOUT_KEY];
			if (layoutKey) foundKeys.push(layoutKey);
			return true;
		});

		// 'inline-1' via the legacy data-key fallback in getBlockAttrs
		expect(foundKeys).to.deep.equal(['layout-1', 'inline-1']);
	});
});
