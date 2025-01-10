import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardContext } from './clipboard.context.js';
import { UmbClipboardEntryDetailStore, type UmbClipboardEntryDetailModel } from '../clipboard-entry/index.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../clipboard-entry/entity.js';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbClipboardCopyPropertyValueTranslator,
	UmbClipboardPastePropertyValueTranslator,
} from '../property-value-translator/types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const TEST_PROPERTY_EDITOR_UI_ALIAS = 'testPropertyEditorUiAlias';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE = 'testClipboardEntryValueType';

class UmbTestClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<string, string>
{
	async translate(propertyValue: string): Promise<string> {
		const cleanedValue = propertyValue.replaceAll(' property value', '');
		return cleanedValue + ' clipboard value';
	}
}

const copyTranslatorManifest = {
	type: 'clipboardCopyPropertyValueTranslator',
	alias: 'Test.ClipboardCopyPropertyValueTranslator1',
	name: 'Test Clipboard Copy Property Value Translator 1',
	api: UmbTestClipboardCopyPropertyValueTranslator,
	fromPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
	toClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE,
};

class UmbTestClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<string, string>
{
	async translate(clipboardEntryValue: string): Promise<string> {
		const cleanedValue = clipboardEntryValue.replaceAll(' clipboard value', '');
		return cleanedValue + ' property value';
	}
}

const pasteTranslatorManifest = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Test.ClipboardPastePropertyValueTranslator1',
	name: 'Test Clipboard Paste Property Value Translator 1',
	api: UmbTestClipboardPastePropertyValueTranslator,
	weight: 1,
	fromClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE,
	toPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
};

const propertyEditorManifest = {
	type: 'propertyEditorUi',
	alias: TEST_PROPERTY_EDITOR_UI_ALIAS,
	name: 'Test Property Editor UI',
	meta: {
		label: 'Test Property Editor',
		icon: 'document',
		group: 'Common',
		propertyEditorSchemaAlias: 'Umbraco.TextBox',
	},
};

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbClipboardEntryDetailStore(this);
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbClipboardContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let clipboardContext: UmbClipboardContext;

	beforeEach(async () => {
		localStorage.clear();
		hostElement = new UmbTestControllerHostElement();
		clipboardContext = new UmbClipboardContext(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	describe('read and write', () => {
		it('should write an entry to the clipboard', async () => {
			const preset: Partial<UmbClipboardEntryDetailModel> = {
				values: [{ type: 'test', value: 'test1' }],
				icon: 'icon1',
				meta: {},
				name: 'Test1',
			};

			const entry = await clipboardContext.write(preset);
			expect(entry?.name).to.equal('Test1');
		});
	});

	describe('read', () => {
		it('should read an entry from the clipboard', async () => {
			const preset: Partial<UmbClipboardEntryDetailModel> = {
				values: [{ type: 'test', value: 'test1' }],
				icon: 'icon1',
				meta: {},
				name: 'Test1',
			};

			const entry = await clipboardContext.write(preset);
			const read = await clipboardContext.read(entry!.unique);
			expect(read?.name).to.equal('Test1');
		});
	});

	describe('clipboard for property values', () => {
		describe('writeForProperty', () => {
			let clipboardEntry: UmbClipboardEntryDetailModel | undefined;

			beforeEach(async () => {
				umbExtensionsRegistry.registerMany([pasteTranslatorManifest, copyTranslatorManifest, propertyEditorManifest]);

				clipboardEntry = await clipboardContext.writeForProperty({
					name: 'Test1',
					icon: 'icon1',
					propertyValue: 'test1',
					propertyEditorUiAlias: TEST_PROPERTY_EDITOR_UI_ALIAS,
				});
			});

			afterEach(() => {
				umbExtensionsRegistry.clear();
			});

			it('should read an entry from the clipboard for a property', async () => {
				expect(clipboardEntry?.name).to.equal('Test1');
				expect(clipboardEntry?.values[0].type).to.equal(TEST_CLIPBOARD_ENTRY_VALUE_TYPE);
				expect(clipboardEntry?.values[0].value).to.equal('test1 clipboard value');
			});

			it('should read an entry from the clipboard for a property', async () => {
				const propertyValue = await clipboardContext.readForProperty<string>(
					clipboardEntry!.unique,
					TEST_PROPERTY_EDITOR_UI_ALIAS,
				);
				expect(propertyValue).to.equal('test1 property value');
			});
		});
	});

	describe('getPastePropertyValueTranslatorManifests', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.registerMany([pasteTranslatorManifest]);
		});

		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		it('should return the paste property value translator manifests', () => {
			const manifests = clipboardContext.getPastePropertyValueTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			expect(manifests).to.have.lengthOf(1);
			expect(manifests[0].alias).to.equal(pasteTranslatorManifest.alias);
		});
	});

	describe('hasSupportedPastePropertyValueTranslator', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.registerMany([pasteTranslatorManifest]);
		});

		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		it('should return true if a supported paste property value translator is available', () => {
			const manifests = clipboardContext.getPastePropertyValueTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			const values = [{ type: TEST_CLIPBOARD_ENTRY_VALUE_TYPE, value: 'test clipboard value' }];
			const hasSupported = clipboardContext.hasSupportedPastePropertyValueTranslator(manifests, values);
			expect(hasSupported).to.be.true;
		});

		it('should return false if no supported paste property value translator is available', () => {
			const manifests = clipboardContext.getPastePropertyValueTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			const values = [{ type: 'unsupported', value: 'test clipboard value' }];
			const hasSupported = clipboardContext.hasSupportedPastePropertyValueTranslator(manifests, values);
			expect(hasSupported).to.be.false;
		});
	});
});
