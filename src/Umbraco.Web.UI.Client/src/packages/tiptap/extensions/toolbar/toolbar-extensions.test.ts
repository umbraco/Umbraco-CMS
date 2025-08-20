import UmbTiptapToolbarBoldExtensionApi from './bold.tiptap-toolbar-api.js';
import UmbTiptapToolbarItalicExtensionApi from './italic.tiptap-toolbar-api.js';
import UmbTiptapToolbarUnderlineExtensionApi from './underline.tiptap-toolbar-api.js';
import { expect } from '@open-wc/testing';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

// Mock editor with chain methods for testing
const createMockEditor = () => {
	let chainMethodsCalled: string[] = [];
	const mockChain = {
		focus: () => {
			chainMethodsCalled.push('focus');
			return mockChain;
		},
		toggleBold: () => {
			chainMethodsCalled.push('toggleBold');
			return mockChain;
		},
		toggleItalic: () => {
			chainMethodsCalled.push('toggleItalic');
			return mockChain;
		},
		toggleUnderline: () => {
			chainMethodsCalled.push('toggleUnderline');
			return mockChain;
		},
		run: () => {
			chainMethodsCalled.push('run');
			return mockChain;
		}
	};
	
	return {
		chain: () => mockChain,
		getChainMethodsCalled: () => chainMethodsCalled,
		isActive: (name: string) => false // Default to not active
	} as Editor & { getChainMethodsCalled: () => string[] };
};

describe('UmbTiptapToolbarBoldExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapToolbarBoldExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapToolbarBoldExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapToolbarBoldExtensionApi);
	});

	it('executes bold toggle command', () => {
		const mockEditor = createMockEditor();
		extension.execute(mockEditor);
		
		const calledMethods = mockEditor.getChainMethodsCalled();
		expect(calledMethods).to.include('focus');
		expect(calledMethods).to.include('toggleBold');
		expect(calledMethods).to.include('run');
	});

	it('handles undefined editor gracefully', () => {
		expect(() => extension.execute()).to.not.throw;
	});
});

describe('UmbTiptapToolbarItalicExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapToolbarItalicExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapToolbarItalicExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapToolbarItalicExtensionApi);
	});

	it('executes italic toggle command', () => {
		const mockEditor = createMockEditor();
		extension.execute(mockEditor);
		
		const calledMethods = mockEditor.getChainMethodsCalled();
		expect(calledMethods).to.include('focus');
		expect(calledMethods).to.include('toggleItalic');
		expect(calledMethods).to.include('run');
	});

	it('handles undefined editor gracefully', () => {
		expect(() => extension.execute()).to.not.throw;
	});
});

describe('UmbTiptapToolbarUnderlineExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapToolbarUnderlineExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapToolbarUnderlineExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapToolbarUnderlineExtensionApi);
	});

	it('executes underline toggle command', () => {
		const mockEditor = createMockEditor();
		extension.execute(mockEditor);
		
		const calledMethods = mockEditor.getChainMethodsCalled();
		expect(calledMethods).to.include('focus');
		expect(calledMethods).to.include('toggleUnderline');
		expect(calledMethods).to.include('run');
	});

	it('handles undefined editor gracefully', () => {
		expect(() => extension.execute()).to.not.throw;
	});
});