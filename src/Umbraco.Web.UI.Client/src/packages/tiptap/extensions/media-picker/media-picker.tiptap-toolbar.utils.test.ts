import { extractFigureAttrs, extractFigureImageData, extractImageMarks } from './media-picker.tiptap-toolbar.utils.js';
import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { UmbImage } from '../image/image.tiptap-extension.js';
import { UmbLink } from '../link/link.tiptap-extension.js';
import { Figure } from '../figure/figure.tiptap-extension.js';
import { Figcaption } from '../figure/figcaption.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('media-picker.tiptap-toolbar.utils', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			extensions: [Document, Paragraph, Text, UmbImage.configure({ inline: true }), UmbLink, Figure, Figcaption],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	function findNodePos(typeName: string): number {
		let foundPos = -1;
		editor.state.doc.descendants((node, pos) => {
			if (foundPos !== -1) return false;
			if (node.type.name === typeName) {
				foundPos = pos;
				return false;
			}
			return true;
		});
		return foundPos;
	}

	describe('extractImageMarks', () => {
		it('returns the umbLink mark when an image with a link is selected', () => {
			editor.commands.setContent(
				'<p><a href="https://example.com"><img src="foo.png" data-udi="umb://media/abc"></a></p>',
			);
			editor.commands.setNodeSelection(findNodePos('image'));

			const marks = extractImageMarks(editor.state.selection);

			expect(marks).to.have.lengthOf(1);
			expect(marks[0].type).to.equal('umbLink');
			expect(marks[0].attrs?.href).to.equal('https://example.com');
		});

		it('returns an empty array for an image with no marks', () => {
			editor.commands.setContent('<p><img src="foo.png" data-udi="umb://media/abc"></p>');
			editor.commands.setNodeSelection(findNodePos('image'));

			expect(extractImageMarks(editor.state.selection)).to.deep.equal([]);
		});

		it('returns an empty array when the selection is not a NodeSelection on an image', () => {
			editor.commands.setContent('<p>plain text</p>');
			editor.commands.selectAll();

			expect(extractImageMarks(editor.state.selection)).to.deep.equal([]);
		});

		it('drills into a NodeSelection on a figure to recover the inner image marks', () => {
			// An atomic figure absorbs clicks meant for the inner image: the resulting
			// selection lands on the figure node, not on the image inside it.
			editor.commands.setContent(
				[
					'<figure>',
					'  <p><a href="https://example.com"><img src="foo.png" data-udi="umb://media/abc"></a></p>',
					'  <figcaption>Caption text</figcaption>',
					'</figure>',
				].join(''),
			);
			editor.commands.setNodeSelection(findNodePos('figure'));

			const marks = extractImageMarks(editor.state.selection);

			expect(marks).to.have.lengthOf(1);
			expect(marks[0].type).to.equal('umbLink');
			expect(marks[0].attrs?.href).to.equal('https://example.com');
		});
	});

	describe('extractFigureAttrs', () => {
		it('returns the figure attrs when the selection is a NodeSelection on the figure', () => {
			editor.commands.setContent(
				'<figure figcaption="Mirror text">' +
					'<p><img src="foo.png" data-udi="umb://media/abc"></p>' +
					'<figcaption>Caption text</figcaption>' +
					'</figure>',
			);
			editor.commands.setNodeSelection(findNodePos('figure'));

			expect(extractFigureAttrs(editor)?.figcaption).to.equal('Mirror text');
		});

		it('returns the figure attrs when the cursor is inside the figure', () => {
			editor.commands.setContent(
				'<figure figcaption="Mirror text">' +
					'<p><img src="foo.png" data-udi="umb://media/abc"></p>' +
					'<figcaption>Caption text</figcaption>' +
					'</figure>',
			);
			editor.commands.setTextSelection(findNodePos('figcaption') + 1);

			expect(extractFigureAttrs(editor)?.figcaption).to.equal('Mirror text');
		});

		it('returns undefined when no figure wraps the selection', () => {
			editor.commands.setContent('<p>plain text</p>');
			editor.commands.selectAll();

			expect(extractFigureAttrs(editor)).to.be.undefined;
		});
	});

	describe('extractFigureImageData', () => {
		it('extracts image attrs, caption text, and the umbLink mark from a figure', () => {
			editor.commands.setContent(
				[
					'<figure>',
					'  <p><a href="https://example.com"><img src="foo.png" data-udi="umb://media/abc"></a></p>',
					'  <figcaption>Caption text</figcaption>',
					'</figure>',
				].join(''),
			);

			// Place the text cursor inside the figcaption content (the `+1` skips the figcaption's opening token).
			const figcaptionPos = findNodePos('figcaption');
			expect(figcaptionPos, 'figcaption position').to.be.greaterThan(-1);
			editor.commands.setTextSelection(figcaptionPos + 1);

			const data = extractFigureImageData(editor);

			expect(data, 'figure data').to.exist;
			expect(data!.imageAttrs['data-udi']).to.equal('umb://media/abc');
			expect(data!.caption).to.equal('Caption text');
			expect(data!.marks).to.have.lengthOf(1);
			expect(data!.marks[0].type).to.equal('umbLink');
			expect(data!.marks[0].attrs?.href).to.equal('https://example.com');
		});

		it('also resolves when the selection is a NodeSelection on the figure itself', () => {
			editor.commands.setContent(
				[
					'<figure>',
					'  <p><a href="https://example.com"><img src="foo.png" data-udi="umb://media/abc"></a></p>',
					'  <figcaption>Caption text</figcaption>',
					'</figure>',
				].join(''),
			);
			editor.commands.setNodeSelection(findNodePos('figure'));

			const data = extractFigureImageData(editor);

			expect(data, 'figure data').to.exist;
			expect(data!.imageAttrs['data-udi']).to.equal('umb://media/abc');
			expect(data!.caption).to.equal('Caption text');
			expect(data!.marks[0].type).to.equal('umbLink');
		});

		it('returns undefined when the selection is not inside a figure', () => {
			editor.commands.setContent('<p>plain text</p>');
			editor.commands.selectAll();

			expect(extractFigureImageData(editor)).to.be.undefined;
		});

		it('returns undefined when a figure contains no image (only a caption)', () => {
			editor.commands.setContent('<figure><p>no image</p><figcaption>cap</figcaption></figure>');

			const figcaptionPos = findNodePos('figcaption');
			editor.commands.setTextSelection(figcaptionPos + 1);

			expect(extractFigureImageData(editor)).to.be.undefined;
		});
	});

	describe('round-trip via insertContent', () => {
		it('re-inserting an image with the extracted marks keeps the surrounding link', () => {
			editor.commands.setContent(
				'<p><a href="https://example.com"><img src="foo.png" data-udi="umb://media/abc"></a></p>',
			);
			editor.commands.setNodeSelection(findNodePos('image'));

			const marks = extractImageMarks(editor.state.selection);

			editor.commands.insertContent({
				type: 'image',
				attrs: { src: 'bar.png', 'data-udi': 'umb://media/abc' },
				marks,
			});

			expect(editor.getHTML()).to.match(/<a[^>]*href="https:\/\/example\.com"[^>]*>\s*<img[^>]+src="bar\.png"/);
		});

		it('re-inserting a figure with the extracted marks on the inner image keeps the surrounding link', () => {
			editor.commands.setContent(
				[
					'<figure>',
					'  <p><a href="https://example.com"><img src="foo.png" data-udi="umb://media/abc"></a></p>',
					'  <figcaption>Original caption</figcaption>',
					'</figure>',
				].join(''),
			);

			const figcaptionPos = findNodePos('figcaption');
			editor.commands.setTextSelection(figcaptionPos + 1);

			const figureData = extractFigureImageData(editor);
			expect(figureData, 'figure data').to.exist;
			editor.commands.setNodeSelection(figureData!.pos);

			editor.commands.insertContent({
				type: 'figure',
				content: [
					{
						type: 'paragraph',
						content: [
							{
								type: 'image',
								attrs: { src: 'bar.png', 'data-udi': 'umb://media/abc' },
								marks: figureData!.marks,
							},
						],
					},
					{ type: 'figcaption', content: [{ type: 'text', text: 'New caption' }] },
				],
			});

			expect(editor.getHTML(), 'figure HTML').to.match(
				/<a[^>]*href="https:\/\/example\.com"[^>]*>\s*<img[^>]+src="bar\.png"/,
			);
		});

		it('re-applying the captured figure attrs through insertContent preserves them', () => {
			editor.commands.setContent(
				'<figure figcaption="Mirror text">' +
					'<p><img src="foo.png" data-udi="umb://media/abc"></p>' +
					'<figcaption>Caption text</figcaption>' +
					'</figure>',
			);
			editor.commands.setNodeSelection(findNodePos('figure'));
			const attrs = extractFigureAttrs(editor);

			editor.commands.insertContent({
				type: 'figure',
				attrs,
				content: [
					{ type: 'paragraph', content: [{ type: 'image', attrs: { src: 'bar.png', 'data-udi': 'umb://media/abc' } }] },
					{ type: 'figcaption', content: [{ type: 'text', text: 'New caption' }] },
				],
			});

			expect(editor.getHTML()).to.include('figcaption="Mirror text"');
		});
	});
});
