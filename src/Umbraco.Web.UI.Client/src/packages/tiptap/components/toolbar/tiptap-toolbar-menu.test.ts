import { UmbTiptapToolbarMenuElement } from './tiptap-toolbar-menu.element.js';
import type { UmbTiptapToolbarElementApi } from '../../extensions/types.js';
import type { Editor } from '../../externals.js';
import { expect } from '@open-wc/testing';

/**
 * Editor stub whose `isDisabled` condition (caret inside a list) is independent of
 * `isActive` (which is pinned to `false`). Mirrors the button-element stub used for
 * the #23823 regression tests — `UmbTiptapToolbarMenuElement` wires up `transaction`
 * separately from `UmbTiptapToolbarButtonElement`, so it needs its own coverage.
 */
function makeListAwareEditorStub() {
	const listeners = new Map<string, Array<() => void>>();
	let inList = false;

	return {
		on(event: string, fn: () => void) {
			const bucket = listeners.get(event) ?? [];
			bucket.push(fn);
			listeners.set(event, bucket);
		},
		off(event: string, fn: () => void) {
			const bucket = listeners.get(event) ?? [];
			listeners.set(event, bucket.filter((f) => f !== fn));
		},
		get inList() {
			return inList;
		},
		/** Simulates the caret moving in/out of a list, independent of any mark. */
		fireTransaction() {
			inList = !inList;
			for (const fn of listeners.get('transaction') ?? []) fn();
		},
	};
}

describe('UmbTiptapToolbarMenuElement — disabled state (#23823)', () => {
	let editorStub: ReturnType<typeof makeListAwareEditorStub>;
	let element: UmbTiptapToolbarMenuElement;

	const listAwareApi: UmbTiptapToolbarElementApi = {
		isActive: () => false,
		isDisabled: (e?: Editor) => (e as any)?.inList === true,
		execute: () => {},
	} as unknown as UmbTiptapToolbarElementApi;

	const styleMenuManifest = {
		type: 'tiptapToolbarExtension',
		kind: 'menu',
		alias: 'Umb.Tiptap.Toolbar.StyleMenu',
		name: 'Style Menu',
		meta: { alias: 'styleMenu', label: 'Style', icon: 'icon-text' },
		items: [],
	} as any;

	const isDisabled = () => element.shadowRoot?.querySelector('uui-button')?.hasAttribute('disabled');

	beforeEach(() => {
		editorStub = makeListAwareEditorStub();

		element = document.createElement('umb-tiptap-toolbar-menu') as UmbTiptapToolbarMenuElement;
		// editor must be assigned before connectedCallback so the listener is wired on connect.
		element.editor = editorStub as unknown as Editor;
		element.api = listAwareApi;
		element.manifest = styleMenuManifest;
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
	});

	it('re-evaluates isDisabled on transaction even when isActive does not change', async () => {
		await element.updateComplete;
		expect(isDisabled()).to.equal(false);

		editorStub.fireTransaction(); // caret enters a list — isActive stays false throughout
		await element.updateComplete;

		expect(isDisabled()).to.equal(true);
	});

	it('re-enables once the disabling condition clears', async () => {
		editorStub.fireTransaction();
		await element.updateComplete;
		expect(isDisabled()).to.equal(true);

		editorStub.fireTransaction(); // caret leaves the list
		await element.updateComplete;

		expect(isDisabled()).to.equal(false);
	});

	it('seeds the disabled state on connect, before any transaction fires', async () => {
		const alwaysDisabledApi: UmbTiptapToolbarElementApi = {
			isActive: () => false,
			isDisabled: () => true,
			execute: () => {},
		} as unknown as UmbTiptapToolbarElementApi;

		const seededElement = document.createElement('umb-tiptap-toolbar-menu') as UmbTiptapToolbarMenuElement;
		seededElement.editor = editorStub as unknown as Editor;
		seededElement.api = alwaysDisabledApi;
		seededElement.manifest = styleMenuManifest;
		document.body.appendChild(seededElement);

		await seededElement.updateComplete;
		expect(seededElement.shadowRoot?.querySelector('uui-button')?.hasAttribute('disabled')).to.equal(true);

		seededElement.remove();
	});
});
