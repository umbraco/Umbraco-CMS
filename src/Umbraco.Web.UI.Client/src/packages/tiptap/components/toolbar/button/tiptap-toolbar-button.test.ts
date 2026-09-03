import { UmbTiptapToolbarButtonElement } from './tiptap-toolbar-button.element.js';
import type { UmbTiptapToolbarElementApi } from '../../../extensions/types.js';
import type { Editor } from '../../../externals.js';
import { expect } from '@open-wc/testing';

/**
 * Minimal editor stub that records `on`/`off` subscriptions and lets tests fire a
 * `transaction` event directly. Using a stub (rather than a real `Editor`) avoids
 * Tiptap's DOM-mounting side-effects and keeps the test focused on the listener wiring.
 */
function makeEditorStub() {
	const listeners = new Map<string, Array<() => void>>();
	let markActive = false;

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
		isActive(name: string) {
			return name === 'bold' && markActive;
		},
		/** Simulates a stored-mark transaction: toggles the mark and notifies all `transaction` subscribers. */
		fireTransaction() {
			markActive = !markActive;
			for (const fn of listeners.get('transaction') ?? []) fn();
		},
		/** Returns the registered listener count for the given event name. */
		listenerCount(event: string) {
			return listeners.get(event)?.length ?? 0;
		},
	};
}

describe('UmbTiptapToolbarButtonElement', () => {
	let editorStub: ReturnType<typeof makeEditorStub>;
	let element: UmbTiptapToolbarButtonElement;

	const boldApi: UmbTiptapToolbarElementApi = {
		isActive: (e?: Editor) => (e as any)?.isActive('bold') === true,
		isDisabled: () => false,
		execute: () => {},
	} as unknown as UmbTiptapToolbarElementApi;

	const boldManifest = {
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Bold',
		name: 'Bold',
		meta: { alias: 'bold', label: 'Bold', icon: 'icon-bold' },
	} as any;

	const look = () => element.shadowRoot?.querySelector('uui-button')?.getAttribute('look');

	beforeEach(() => {
		editorStub = makeEditorStub();

		element = document.createElement('umb-tiptap-toolbar-button') as UmbTiptapToolbarButtonElement;
		// editor must be assigned before connectedCallback so the listener is wired on connect.
		element.editor = editorStub as unknown as Editor;
		element.api = boldApi;
		element.manifest = boldManifest;
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
	});

	it('registers a `transaction` listener on connect (not selectionUpdate/update) (#22907)', () => {
		expect(editorStub.listenerCount('transaction')).to.equal(1);
		expect(editorStub.listenerCount('selectionUpdate')).to.equal(0);
		expect(editorStub.listenerCount('update')).to.equal(0);
	});

	it('activates immediately when a transaction fires with a collapsed-cursor mark toggle (#22907)', async () => {
		await element.updateComplete;
		expect(look()).to.equal('default');

		editorStub.fireTransaction(); // simulates stored-mark toggle — no docChanged, no selection change
		await element.updateComplete;

		expect(look()).to.equal('outline');
	});

	it('deactivates when the mark is toggled off again', async () => {
		editorStub.fireTransaction();
		await element.updateComplete;
		expect(look()).to.equal('outline');

		editorStub.fireTransaction();
		await element.updateComplete;

		expect(look()).to.equal('default');
	});

	it('removes the `transaction` listener on disconnect', () => {
		expect(editorStub.listenerCount('transaction')).to.equal(1);

		element.remove();

		expect(editorStub.listenerCount('transaction')).to.equal(0);
	});
});

/**
 * Editor stub whose `isDisabled` condition (caret inside a list) is independent of
 * `isActive` (which is pinned to `false`). Isolates the #23823 bug: `isDisabled` must
 * re-evaluate on every `transaction`, not only as a side effect of `isActive` changing.
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

describe('UmbTiptapToolbarButtonElement — disabled state (#23823)', () => {
	let editorStub: ReturnType<typeof makeListAwareEditorStub>;
	let element: UmbTiptapToolbarButtonElement;

	const listAwareApi: UmbTiptapToolbarElementApi = {
		isActive: () => false,
		isDisabled: (e?: Editor) => (e as any)?.inList === true,
		execute: () => {},
	} as unknown as UmbTiptapToolbarElementApi;

	const textAlignManifest = {
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignCenter',
		name: 'Text Align Center',
		meta: { alias: 'textAlignCenter', label: 'Align center', icon: 'icon-text-align-center' },
	} as any;

	const isDisabled = () => element.shadowRoot?.querySelector('uui-button')?.hasAttribute('disabled');

	beforeEach(() => {
		editorStub = makeListAwareEditorStub();

		element = document.createElement('umb-tiptap-toolbar-button') as UmbTiptapToolbarButtonElement;
		element.editor = editorStub as unknown as Editor;
		element.api = listAwareApi;
		element.manifest = textAlignManifest;
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

		const seededElement = document.createElement('umb-tiptap-toolbar-button') as UmbTiptapToolbarButtonElement;
		seededElement.editor = editorStub as unknown as Editor;
		seededElement.api = alwaysDisabledApi;
		seededElement.manifest = textAlignManifest;
		document.body.appendChild(seededElement);

		await seededElement.updateComplete;
		expect(seededElement.shadowRoot?.querySelector('uui-button')?.hasAttribute('disabled')).to.equal(true);

		seededElement.remove();
	});
});
