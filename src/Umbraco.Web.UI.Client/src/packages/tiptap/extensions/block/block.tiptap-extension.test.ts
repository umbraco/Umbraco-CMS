import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
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
});
