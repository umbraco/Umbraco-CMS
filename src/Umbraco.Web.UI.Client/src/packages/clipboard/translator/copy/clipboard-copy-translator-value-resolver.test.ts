import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardCopyTranslatorValueResolver } from './clipboard-copy-translator-value-resolver.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from './types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbClipboardEntryValueModel } from '../../clipboard-entry';

const TEST_PROPERTY_EDITOR_UI_ALIAS = 'testPropertyEditorUiAlias';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1 = 'testClipboardEntryValueType1';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2 = 'testClipboardEntryValueType2';

type TestValueType = String;

class UmbTestClipboardCopyTranslator1
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<TestValueType, TestValueType>
{
	async translate(value: TestValueType): Promise<TestValueType> {
		return value + '1';
	}
}

class UmbTestClipboardCopyTranslator2
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<TestValueType, TestValueType>
{
	async translate(value: TestValueType): Promise<TestValueType> {
		return value + '2';
	}
}

const copyTranslatorManifest1 = {
	type: 'clipboardCopyTranslator',
	alias: 'Test.ClipboardCopyTranslator1',
	name: 'Test Clipboard Copy Translator 1',
	api: UmbTestClipboardCopyTranslator1,
	fromPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
	toClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_1,
};

const copyTranslatorManifest2 = {
	...copyTranslatorManifest1,
	alias: 'Test.ClipboardCopyTranslator2',
	name: 'Test Clipboard Copy Translator 2',
	api: UmbTestClipboardCopyTranslator2,
	toClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE_2,
};

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbClipboardCopyTranslatorValueResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbClipboardCopyTranslatorValueResolver;

	const propertyValue = 'testValue';

	beforeEach(async () => {
		umbExtensionsRegistry.clear();
		umbExtensionsRegistry.registerMany([copyTranslatorManifest1, copyTranslatorManifest2]);
		hostElement = new UmbTestControllerHostElement();
		resolver = new UmbClipboardCopyTranslatorValueResolver(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
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
