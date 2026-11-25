import { Bold, Document, Editor, Italic, Link, Node, Paragraph, Text } from '../../externals.js';
import { UmbTrimMarkWhitespace } from './trim-mark-whitespace.tiptap-extension.js';
import { expect } from '@open-wc/testing';

const InlineAtom = Node.create({
	name: 'inlineAtom',
	group: 'inline',
	inline: true,
	atom: true,
	parseHTML: () => [{ tag: 'span[data-atom]' }],
	renderHTML: () => ['span', { 'data-atom': '' }],
});

describe('UmbTrimMarkWhitespace', () => {
	let editorHost: HTMLDivElement;
	let editor: Editor;

	beforeEach(() => {
		editorHost = document.createElement('div');
		document.body.appendChild(editorHost);
		editor = new Editor({
			element: editorHost,
			extensions: [Document, Paragraph, Text, Bold, Italic, Link, InlineAtom, UmbTrimMarkWhitespace],
		});
	});

	afterEach(() => {
		editor.destroy();
		editorHost.remove();
	});

	const select = (html: string, from: number, to: number) => {
		editor.commands.setContent(html);
		editor.commands.setTextSelection({ from, to });
	};

	it('excludes trailing whitespace from an applied mark', () => {
		select('<p>This is a bold word</p>', 11, 16);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a <strong>bold</strong> word</p>');
	});

	it('excludes leading whitespace from an applied mark', () => {
		select('<p>This is a bold word</p>', 10, 15);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a <strong>bold</strong> word</p>');
	});

	it('applies to any mark, not only bold', () => {
		select('<p>This is a bold word</p>', 11, 16);
		editor.commands.toggleItalic();
		expect(editor.getHTML()).to.equal('<p>This is a <em>bold</em> word</p>');
	});

	it('marks a whitespace-only selection unchanged', () => {
		select('<p>a b</p>', 2, 3);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>a<strong> </strong>b</p>');
	});

	it('leaves a selection without edge whitespace unchanged', () => {
		select('<p>This is a bold word</p>', 11, 15);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a <strong>bold</strong> word</p>');
	});

	it('still removes a mark from the full selection', () => {
		select('<p>This is a <strong>bold </strong>word</p>', 11, 16);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a bold word</p>');
	});

	it('keeps the original selection after applying a mark', () => {
		select('<p>This is a bold word</p>', 11, 16);
		editor.commands.toggleBold();
		expect(editor.state.selection.from).to.equal(11);
		expect(editor.state.selection.to).to.equal(16);
	});

	it('marks the whole range when the selection ends at the start of the next block', () => {
		editor.commands.setContent('<p>abc  </p><p>def</p>', { parseOptions: { preserveWhitespace: 'full' } });
		editor.commands.setTextSelection({ from: 1, to: 8 });
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p><strong>abc  </strong></p><p>def</p>');
	});

	it('keeps an inline leaf at the selection edge inside the mark', () => {
		select('<p><span data-atom></span> text</p>', 1, 4);
		editor.commands.toggleBold();
		expect(editor.state.doc.nodeAt(1)?.marks.map((mark) => mark.type.name)).to.deep.equal(['bold']);
	});

	describe('toggling an existing mark', () => {
		const boldWord = '<p>This is a <strong>bold</strong> word</p>';

		it('removes the mark when the selection includes unmarked trailing whitespace', () => {
			select(boldWord, 11, 16);
			editor.commands.toggleBold();
			expect(editor.getHTML()).to.equal('<p>This is a bold word</p>');
		});

		it('keeps the original selection after removing the mark', () => {
			select(boldWord, 11, 16);
			editor.commands.toggleBold();
			expect(editor.state.selection.from).to.equal(11);
			expect(editor.state.selection.to).to.equal(16);
		});
	});

	describe('double-click', () => {
		const boldWord = '<p>This is a <strong>bold</strong> word</p>';
		const doubleClick = async () => {
			editor.view.dom.dispatchEvent(new MouseEvent('dblclick', { bubbles: true }));
			await new Promise((resolve) => setTimeout(resolve));
		};

		it('excludes trailing whitespace from the selection', async () => {
			select(boldWord, 11, 16);
			await doubleClick();
			expect(editor.state.selection.from).to.equal(11);
			expect(editor.state.selection.to).to.equal(15);
		});

		it('makes the mark active for the selected word', async () => {
			select(boldWord, 11, 16);
			expect(editor.isActive('bold')).to.be.false;
			await doubleClick();
			expect(editor.isActive('bold')).to.be.true;
		});

		it('leaves a selection without edge whitespace unchanged', async () => {
			select(boldWord, 11, 15);
			await doubleClick();
			expect(editor.state.selection.from).to.equal(11);
			expect(editor.state.selection.to).to.equal(15);
		});

		it('keeps whitespace that already carries a mark, so the whole run can be unset', async () => {
			select('<p>This is a <strong>bold </strong>word</p>', 11, 16);
			await doubleClick();
			expect(editor.state.selection.to).to.equal(16);
			editor.commands.toggleBold();
			expect(editor.getHTML()).to.equal('<p>This is a bold word</p>');
		});
	});

	describe('updating an existing mark', () => {
		it('keeps trailing whitespace that already has the mark in the updated range', () => {
			editor.commands.setContent('<p>See <a href="/old">this link </a>here.</p>');
			editor.commands.setTextSelection(7);
			editor.chain().extendMarkRange('link').setLink({ href: '/new' }).run();
			expect(editor.getHTML()).to.contain('href="/new">this link </a>here.');
			expect(editor.getHTML()).to.not.contain('/old');
		});
	});
});
