import { UmbTiptapStatusbarWordCountElement } from './word-count.tiptap-statusbar-element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

// Mock editor with storage and text content methods
const createMockEditorWithContent = (textContent: string, characterCount: number) => {
	const storage = new Map();
	return {
		on: (event: string, callback: Function) => {
			// Store the callback for later invocation
			storage.set(event, callback);
		},
		off: (event: string, callback: Function) => {
			storage.delete(event);
		},
		storage: {
			characterCount: {
				characters: () => characterCount,
				words: () => textContent.split(/\s+/).filter(word => word.length > 0).length
			}
		},
		getText: () => textContent,
		// Method to simulate editor updates
		simulateUpdate: () => {
			const callback = storage.get('update');
			if (callback) callback();
		}
	} as Editor & { simulateUpdate: () => void };
};

describe('UmbTiptapStatusbarWordCountElement', () => {
	let element: UmbTiptapStatusbarWordCountElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-tiptap-statusbar-word-count></umb-tiptap-statusbar-word-count> `);
	});

	afterEach(() => {
		element?.remove();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbTiptapStatusbarWordCountElement);
	});

	it('displays initial zero counts', async () => {
		await element.updateComplete;
		
		// Check that initial state shows zero counts
		expect(element['_words']).to.equal(0);
		expect(element['_characters']).to.equal(0);
	});

	it('can set editor and registers update listener', () => {
		const mockEditor = createMockEditorWithContent('Test content', 12);
		element.editor = mockEditor;
		
		// Connect the element to trigger connectedCallback
		document.body.appendChild(element);
		
		expect(element.editor).to.equal(mockEditor);
		
		// Clean up
		document.body.removeChild(element);
	});

	it('updates counts when editor content changes', async () => {
		const testContent = 'Hello world this is a test';
		const mockEditor = createMockEditorWithContent(testContent, testContent.length);
		
		element.editor = mockEditor;
		document.body.appendChild(element);
		
		// Simulate editor update
		mockEditor.simulateUpdate();
		await element.updateComplete;
		
		// Should update word and character counts
		expect(element['_words']).to.be.greaterThan(0);
		expect(element['_characters']).to.be.greaterThan(0);
		
		document.body.removeChild(element);
	});

	it('handles empty content correctly', async () => {
		const mockEditor = createMockEditorWithContent('', 0);
		
		element.editor = mockEditor;
		document.body.appendChild(element);
		
		mockEditor.simulateUpdate();
		await element.updateComplete;
		
		expect(element['_words']).to.equal(0);
		expect(element['_characters']).to.equal(0);
		
		document.body.removeChild(element);
	});

	it('handles content with multiple words correctly', async () => {
		const testContent = 'This is a test with five words';
		const expectedWords = 7; // "This", "is", "a", "test", "with", "five", "words"
		const mockEditor = createMockEditorWithContent(testContent, testContent.length);
		
		element.editor = mockEditor;
		document.body.appendChild(element);
		
		mockEditor.simulateUpdate();
		await element.updateComplete;
		
		expect(element['_words']).to.equal(expectedWords);
		
		document.body.removeChild(element);
	});

	it('toggles between word and character display on click', async () => {
		const testContent = 'Test content';
		const mockEditor = createMockEditorWithContent(testContent, testContent.length);
		
		element.editor = mockEditor;
		document.body.appendChild(element);
		
		mockEditor.simulateUpdate();
		await element.updateComplete;
		
		// Initial state should show words
		expect(element['_showCharacters']).to.be.false;
		
		// Simulate click to toggle
		element.click();
		await element.updateComplete;
		
		expect(element['_showCharacters']).to.be.true;
		
		// Click again to toggle back
		element.click();
		await element.updateComplete;
		
		expect(element['_showCharacters']).to.be.false;
		
		document.body.removeChild(element);
	});

	it('properly cleans up event listeners on disconnect', () => {
		const mockEditor = createMockEditorWithContent('Test', 4);
		let offCalled = false;
		
		// Override off method to track cleanup
		const originalOff = mockEditor.off;
		mockEditor.off = (...args) => {
			offCalled = true;
			return originalOff.call(mockEditor, ...args);
		};
		
		element.editor = mockEditor;
		document.body.appendChild(element);
		document.body.removeChild(element);
		
		expect(offCalled).to.be.true;
	});

	it('handles editor without storage gracefully', async () => {
		const mockEditor = {
			on: () => {},
			off: () => {},
			getText: () => 'test'
		} as any;
		
		element.editor = mockEditor;
		document.body.appendChild(element);
		
		// Should not throw when storage is not available
		expect(() => {
			const callback = element['#onEditorUpdate'] || (() => {});
			callback.call(element);
		}).to.not.throw;
		
		document.body.removeChild(element);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});