import { UmbTiptapRteContext } from './tiptap-rte.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('test-tiptap-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTiptapRteContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTiptapRteContext;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbTiptapRteContext(hostElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		context.destroy();
		document.body.innerHTML = '';
	});

	it('is defined with its own instance', () => {
		expect(context).to.be.instanceOf(UmbTiptapRteContext);
	});

	it('returns undefined editor initially', () => {
		const editor = context.getEditor();
		expect(editor).to.be.undefined;
	});

	it('can set and get editor', () => {
		const mockEditor = {} as Editor;
		context.setEditor(mockEditor);
		
		const retrievedEditor = context.getEditor();
		expect(retrievedEditor).to.equal(mockEditor);
	});

	it('can clear editor by setting undefined', () => {
		const mockEditor = {} as Editor;
		context.setEditor(mockEditor);
		expect(context.getEditor()).to.equal(mockEditor);
		
		context.setEditor(undefined);
		expect(context.getEditor()).to.be.undefined;
	});

	it('can replace editor', () => {
		const mockEditor1 = { id: 'editor1' } as Editor & { id: string };
		const mockEditor2 = { id: 'editor2' } as Editor & { id: string };
		
		context.setEditor(mockEditor1);
		expect(context.getEditor()).to.equal(mockEditor1);
		
		context.setEditor(mockEditor2);
		expect(context.getEditor()).to.equal(mockEditor2);
	});
});