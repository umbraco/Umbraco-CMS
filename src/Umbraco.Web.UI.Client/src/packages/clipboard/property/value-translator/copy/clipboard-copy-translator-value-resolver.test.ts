import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardCopyPropertyValueTranslatorValueResolver } from './clipboard-copy-translator-value-resolver.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from './types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbClipboardEntryValueModel } from '../../../clipboard-entry/index.js';

const TEST_PROPERTY_EDITOR_UI_ALIAS = 'testPropertyEditorUiAlias';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1 = 'testClipboardEntryValueType1';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2 = 'testClipboardEntryValueType2';

type TestValueType = String;

class UmbTestClipboardCopyPropertyValueTranslator1
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<TestValueType, TestValueType>
{
	async translate(value: TestValueType): Promise<TestValueType> {
		return value + '1';
	}
}

class UmbTestClipboardCopyPropertyValueTranslator2
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<TestValueType, TestValueType>
{
	async translate(value: TestValueType): Promise<TestValueType> {
		return value + '2';
	}
}

const copyTranslatorManifest1 = {
	type: 'clipboardCopyPropertyValueTranslator',
	alias: 'Test.ClipboardCopyPropertyValueTranslator1',
	name: 'Test Clipboard Copy Property Value Translator 1',
	api: UmbTestClipboardCopyPropertyValueTranslator1,
	fromPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
	toClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1,
};

const copyTranslatorManifest2 = {
	type: 'clipboardCopyPropertyValueTranslator',
	alias: 'Test.ClipboardCopyPropertyValueTranslator2',
	name: 'Test Clipboard Copy Property Value Translator 2',
	api: UmbTestClipboardCopyPropertyValueTranslator2,
	fromPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
	toClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2,
};

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbClipboardCopyPropertyValueTranslatorValueResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbClipboardCopyPropertyValueTranslatorValueResolver;

	const propertyValue = 'testValue';

	beforeEach(async () => {
		umbExtensionsRegistry.registerMany([copyTranslatorManifest1, copyTranslatorManifest2]);
		hostElement = new UmbTestControllerHostElement();
		resolver = new UmbClipboardCopyPropertyValueTranslatorValueResolver(hostElement);
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
		let clipboardEntryValue: Array<UmbClipboardEntryValueModel<TestValueType>>;

		beforeEach(async () => {
			clipboardEntryValue = await resolver.resolve(propertyValue, TEST_PROPERTY_EDITOR_UI_ALIAS);
		});

		it('returns an array of values', async () => {
			expect(clipboardEntryValue).length(2);
		});

		it('includes entries with the types of the registered translators', () => {
			expect(clipboardEntryValue[0].type).to.equal(TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1);
			expect(clipboardEntryValue[1].type).to.equal(TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2);
		});

		it('includes entries with the values by the registered translators', () => {
			expect(clipboardEntryValue[0].value).to.equal(propertyValue + '1');
			expect(clipboardEntryValue[1].value).to.equal(propertyValue + '2');
		});
	});
});
