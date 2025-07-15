import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardPastePropertyValueTranslatorValueResolver } from './clipboard-paste-translator-value-resolver.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from './types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { type UmbClipboardEntryValuesType } from '../../../clipboard-entry/index.js';

const TEST_PROPERTY_EDITOR_UI_ALIAS = 'testPropertyEditorUiAlias';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1 = 'testClipboardEntryValueType1';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2 = 'testClipboardEntryValueType2';

type TestValueType = string;

class UmbTestClipboardPastePropertyValueTranslator1
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<TestValueType, TestValueType>
{
	async translate(value: TestValueType): Promise<TestValueType> {
		return value + '1';
	}
}

class UmbTestClipboardPastePropertyValueTranslator2
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<TestValueType, TestValueType>
{
	async translate(value: TestValueType): Promise<TestValueType> {
		return value + '2';
	}
}

const pasteTranslatorManifest1 = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Test.ClipboardPastePropertyValueTranslator1',
	name: 'Test Clipboard Paste Property Value Translator 1',
	api: UmbTestClipboardPastePropertyValueTranslator1,
	weight: 1,
	fromClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1,
	toPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
};

const pasteTranslatorManifest2 = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Test.ClipboardPastePropertyValueTranslator2',
	name: 'Test Clipboard Paste Property Value Translator 2',
	api: UmbTestClipboardPastePropertyValueTranslator2,
	weight: 2,
	fromClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2,
	toPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
};

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbClipboardCopyPropertyValueTranslatorValueResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbClipboardPastePropertyValueTranslatorValueResolver<string>;

	const clipboardEntryValues: UmbClipboardEntryValuesType = [
		{
			type: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1,
			value: 'testValue',
		},
		{
			type: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2,
			value: 'testValue',
		},
	];

	beforeEach(async () => {
		umbExtensionsRegistry.registerMany([pasteTranslatorManifest1, pasteTranslatorManifest2]);
		hostElement = new UmbTestControllerHostElement();
		resolver = new UmbClipboardPastePropertyValueTranslatorValueResolver<string>(hostElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		umbExtensionsRegistry.clear();
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a resolve method', () => {
				expect(resolver).to.have.property('resolve').that.is.a('function');
			});
		});
	});

	describe('resolve', async () => {
		let propertyValue: string | undefined;

		beforeEach(async () => {
			propertyValue = await resolver.resolve(clipboardEntryValues, TEST_PROPERTY_EDITOR_UI_ALIAS);
		});

		it('should return the property value translated by the paste translator with the highest weight', async () => {
			await expect(propertyValue).to.equal('testValue2');
		});
	});
});
