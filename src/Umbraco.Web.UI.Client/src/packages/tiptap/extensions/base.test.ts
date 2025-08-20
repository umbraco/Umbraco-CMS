import { UmbTiptapExtensionApiBase, UmbTiptapToolbarElementApiBase } from './base.js';
import { expect } from '@open-wc/testing';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

// Test controller host element
@customElement('test-tiptap-extension-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Mock editor for testing
const createMockEditor = (isActiveResult = false): Editor => ({
	isActive: () => isActiveResult,
	// Add other editor methods as needed for tests
} as any);

// Concrete implementation for testing abstract base class
class TestTiptapExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [];
}

// Concrete implementation for testing abstract toolbar base class
class TestTiptapToolbarElementApi extends UmbTiptapToolbarElementApiBase {
	execute = () => {};
}

describe('UmbTiptapExtensionApiBase', () => {
	let host: UmbTestControllerHostElement;
	let extension: TestTiptapExtensionApi;

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		extension = new TestTiptapExtensionApi(host);
		document.body.appendChild(host);
	});

	afterEach(() => {
		extension.destroy();
		document.body.removeChild(host);
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapExtensionApiBase);
	});

	it('can set and store editor instance', () => {
		const mockEditor = createMockEditor();
		extension.setEditor(mockEditor);
		expect(extension['_editor']).to.equal(mockEditor);
	});

	it('returns null styles by default', () => {
		const styles = extension.getStyles();
		expect(styles).to.be.null;
	});

	it('has abstract getTiptapExtensions method implemented', () => {
		const extensions = extension.getTiptapExtensions();
		expect(extensions).to.be.an('array');
		expect(extensions).to.have.length(0);
	});

	it('can accept manifest', () => {
		const manifest = {
			type: 'tiptapExtension',
			alias: 'test.extension',
			name: 'Test Extension'
		} as any;
		extension.manifest = manifest;
		expect(extension.manifest).to.equal(manifest);
	});
});

describe('UmbTiptapToolbarElementApiBase', () => {
	let host: HTMLElement;
	let toolbarElement: TestTiptapToolbarElementApi;

	beforeEach(() => {
		host = document.createElement('div');
		toolbarElement = new TestTiptapToolbarElementApi(host);
	});

	afterEach(() => {
		toolbarElement.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(toolbarElement).to.be.instanceOf(UmbTiptapToolbarElementApiBase);
	});

	it('can accept configuration', () => {
		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'extensions', value: ['test.extension'] }
		]);
		toolbarElement.configuration = config;
		expect(toolbarElement.configuration).to.equal(config);
	});

	describe('isActive', () => {
		it('returns false when no editor', () => {
			const result = toolbarElement.isActive();
			expect(result).to.be.false;
		});

		it('returns false when no manifest', () => {
			const mockEditor = createMockEditor();
			const result = toolbarElement.isActive(mockEditor);
			expect(result).to.be.false;
		});

		it('returns false when no manifest alias', () => {
			toolbarElement.manifest = { meta: {} } as any;
			const mockEditor = createMockEditor();
			const result = toolbarElement.isActive(mockEditor);
			expect(result).to.be.false;
		});

		it('returns true when editor isActive returns true', () => {
			toolbarElement.manifest = { 
				meta: { alias: 'test-alias' } 
			} as any;
			const mockEditor = createMockEditor(true);
			const result = toolbarElement.isActive(mockEditor);
			expect(result).to.be.true;
		});

		it('returns false when editor isActive returns false', () => {
			toolbarElement.manifest = { 
				meta: { alias: 'test-alias' } 
			} as any;
			const mockEditor = createMockEditor(false);
			const result = toolbarElement.isActive(mockEditor);
			expect(result).to.be.false;
		});
	});

	describe('isDisabled', () => {
		it('returns true when no editor', () => {
			const result = toolbarElement.isDisabled();
			expect(result).to.be.true;
		});

		it('returns false when no forExtensions specified', () => {
			const mockEditor = createMockEditor();
			const result = toolbarElement.isDisabled(mockEditor);
			expect(result).to.be.false;
		});

		it('returns true when required extension not enabled', () => {
			toolbarElement.manifest = {
				forExtensions: ['required.extension']
			} as any;
			toolbarElement.configuration = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['other.extension'] }
			]);
			const mockEditor = createMockEditor();
			const result = toolbarElement.isDisabled(mockEditor);
			expect(result).to.be.true;
		});

		it('returns false when required extension is enabled', () => {
			toolbarElement.manifest = {
				forExtensions: ['required.extension']
			} as any;
			toolbarElement.configuration = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['required.extension', 'other.extension'] }
			]);
			const mockEditor = createMockEditor();
			const result = toolbarElement.isDisabled(mockEditor);
			expect(result).to.be.false;
		});

		it('returns false when all required extensions are enabled', () => {
			toolbarElement.manifest = {
				forExtensions: ['extension1', 'extension2']
			} as any;
			toolbarElement.configuration = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['extension1', 'extension2', 'extension3'] }
			]);
			const mockEditor = createMockEditor();
			const result = toolbarElement.isDisabled(mockEditor);
			expect(result).to.be.false;
		});
	});

	it('has abstract execute method implemented', () => {
		// Test that execute method exists and can be called
		expect(() => toolbarElement.execute()).to.not.throw;
	});
});