import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPropertyContext, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbClipboardCopyPropertyValueTranslator,
	UmbClipboardPastePropertyValueTranslator,
} from '../value-translator/types.js';
import { UmbClipboardEntryDetailStore, type UmbClipboardEntryDetailModel } from '../../clipboard-entry/index.js';
import { UmbClipboardCollectionRepository } from '../../collection/index.js';
import { UmbClipboardPropertyContext } from './clipboard.property-context.js';
import { UmbClipboardContext } from '../../context/clipboard.context.js';

const TEST_PROPERTY_EDITOR_UI_ALIAS = 'testPropertyEditorUiAlias';
const TEST_PROPERTY_EDITOR_UI_ALIAS_WITHOUT_TRANSLATORS = 'testPropertyEditorUiAliasWithoutTranslators';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE = 'testClipboardEntryValueType';
const TEST_INCOMPATIBLE_CLIPBOARD_ENTRY_VALUE_TYPE = 'testIncompatibleClipboardEntryValueType';
const TEST_UNSUPPORTED_CLIPBOARD_ENTRY_VALUE_TYPE = 'testUnsupportedClipboardEntryValueType';

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

class UmbTestIncompatibleClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<string, string>
{
	async translate(clipboardEntryValue: string): Promise<string> {
		return clipboardEntryValue;
	}

	async isCompatibleValue(): Promise<boolean> {
		return false;
	}
}

const incompatiblePasteTranslatorManifest = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Test.ClipboardPastePropertyValueTranslator.Incompatible',
	name: 'Test Incompatible Clipboard Paste Property Value Translator',
	api: UmbTestIncompatibleClipboardPastePropertyValueTranslator,
	fromClipboardEntryValueType: TEST_INCOMPATIBLE_CLIPBOARD_ENTRY_VALUE_TYPE,
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
		new UmbClipboardContext(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbClipboardPropertyContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let clipboardContext: UmbClipboardPropertyContext;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		clipboardContext = new UmbClipboardPropertyContext(hostElement);
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		localStorage.clear();
		document.body.innerHTML = '';
	});

	describe('clipboard for property values', () => {
		describe('write', () => {
			let clipboardEntry: UmbClipboardEntryDetailModel | undefined;

			beforeEach(async () => {
				umbExtensionsRegistry.registerMany([pasteTranslatorManifest, copyTranslatorManifest, propertyEditorManifest]);

				clipboardEntry = await clipboardContext.write({
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
				const propertyValue = await clipboardContext.read<string>(
					clipboardEntry!.unique,
					TEST_PROPERTY_EDITOR_UI_ALIAS,
				);
				expect(propertyValue).to.equal('test1 property value');
			});
		});
	});

	describe('getPasteTranslatorManifests', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.registerMany([pasteTranslatorManifest]);
		});

		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		it('should return the paste property value translator manifests', () => {
			const manifests = clipboardContext.getPasteTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			expect(manifests).to.have.lengthOf(1);
			expect(manifests[0].alias).to.equal(pasteTranslatorManifest.alias);
		});
	});

	describe('hasSupportedPasteTranslator', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.registerMany([pasteTranslatorManifest]);
		});

		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		it('should return true if a supported paste property value translator is available', () => {
			const manifests = clipboardContext.getPasteTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			const values = [{ type: TEST_CLIPBOARD_ENTRY_VALUE_TYPE, value: 'test clipboard value' }];
			const hasSupported = clipboardContext.hasSupportedPasteTranslator(manifests, values);
			expect(hasSupported).to.be.true;
		});

		it('should return false if no supported paste property value translator is available', () => {
			const manifests = clipboardContext.getPasteTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			const values = [{ type: 'unsupported', value: 'test clipboard value' }];
			const hasSupported = clipboardContext.hasSupportedPasteTranslator(manifests, values);
			expect(hasSupported).to.be.false;
		});
	});

	describe('getCopyTranslatorManifests', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.registerMany([copyTranslatorManifest]);
		});

		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		it('should return the copy property value translator manifests', () => {
			const manifests = clipboardContext.getCopyTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS);
			expect(manifests).to.have.lengthOf(1);
			expect(manifests[0].alias).to.equal(copyTranslatorManifest.alias);
		});

		it('should return nothing for a property editor no copy translator targets', () => {
			const manifests = clipboardContext.getCopyTranslatorManifests(TEST_PROPERTY_EDITOR_UI_ALIAS_WITHOUT_TRANSLATORS);
			expect(manifests).to.have.lengthOf(0);
		});
	});

	// The suites below cover what the context derives from the surrounding property, so they provide their own
	// property and dataset contexts and build their own instance on top of them.
	describe('derived from the surrounding property', () => {
		let derivingContext: UmbClipboardPropertyContext;

		beforeEach(() => {
			umbExtensionsRegistry.registerMany([
				copyTranslatorManifest,
				pasteTranslatorManifest,
				incompatiblePasteTranslatorManifest,
				propertyEditorManifest,
			]);
		});

		afterEach(() => {
			umbExtensionsRegistry.clear();
		});

		// A real UmbPropertyContext is used because it and the clipboard property context share the
		// 'UmbPropertyContext' base alias — only the real implementations carry the api alias that lets both resolve
		// independently, as they do in production.
		function providePropertyContext(options?: { alias?: string | undefined; label?: string }) {
			const propertyContext = new UmbPropertyContext(hostElement);
			// Intentionally no setAlias: the alias that matters here is the editor UI alias from the editor
			// manifest, and leaving the property alias unset keeps UmbPropertyContext from observing the dataset
			// stub's value methods.
			propertyContext.setLabel(options?.label ?? 'My Property');
			const alias = options && 'alias' in options ? options.alias : TEST_PROPERTY_EDITOR_UI_ALIAS;
			propertyContext.setEditorManifest(alias ? ({ alias, meta: { icon: 'icon-document' } } as any) : undefined);
			return propertyContext;
		}

		function provideDatasetContext(name = 'My Workspace') {
			const datasetContext = { getName: () => name, getHostElement: () => hostElement } as any;
			new UmbContextProvider(hostElement, UMB_PROPERTY_DATASET_CONTEXT, datasetContext).hostConnected();
		}

		async function createContext() {
			derivingContext = new UmbClipboardPropertyContext(hostElement);
			await aTimeout(0);
			return derivingContext;
		}

		async function readWrittenEntries() {
			const { data } = await new UmbClipboardCollectionRepository(hostElement).requestCollection({
				types: [TEST_CLIPBOARD_ENTRY_VALUE_TYPE],
			});
			return data?.items ?? [];
		}

		function readCurrent<T>(observable: { subscribe: (cb: (value: T) => void) => { unsubscribe: () => void } }): T {
			let value!: T;
			const subscription = observable.subscribe((next) => (value = next));
			subscription.unsubscribe();
			return value;
		}

		describe('copyAvailable', () => {
			it('is true when a copy translator targets the surrounding property editor', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();
				expect(readCurrent(context.copyAvailable)).to.be.true;
			});

			it('is false when no copy translator targets the surrounding property editor', async () => {
				providePropertyContext({ alias: TEST_PROPERTY_EDITOR_UI_ALIAS_WITHOUT_TRANSLATORS });
				provideDatasetContext();
				const context = await createContext();
				expect(readCurrent(context.copyAvailable)).to.be.false;
			});

			it('is false when the property editor UI alias cannot be resolved', async () => {
				providePropertyContext({ alias: undefined });
				provideDatasetContext();
				const context = await createContext();
				expect(readCurrent(context.copyAvailable)).to.be.false;
			});
		});

		describe('pasteAvailable', () => {
			it('is true when a paste translator targets the surrounding property editor', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();
				expect(readCurrent(context.pasteAvailable)).to.be.true;
			});

			it('is false when no paste translator targets the surrounding property editor', async () => {
				providePropertyContext({ alias: TEST_PROPERTY_EDITOR_UI_ALIAS_WITHOUT_TRANSLATORS });
				provideDatasetContext();
				const context = await createContext();
				expect(readCurrent(context.pasteAvailable)).to.be.false;
			});

			it('is false when the property editor UI alias cannot be resolved', async () => {
				providePropertyContext({ alias: undefined });
				provideDatasetContext();
				const context = await createContext();
				expect(readCurrent(context.pasteAvailable)).to.be.false;
			});
		});

		describe('write', () => {
			it('resolves the copy translator from the derived alias', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				await context.write({ propertyValue: 'hello', itemName: 'My Item', icon: 'icon-picture' });

				const entries = await readWrittenEntries();
				expect(entries).to.have.lengthOf(1);
				expect(entries[0].icon).to.equal('icon-picture');
				// The value type is only produced when the alias resolves the copy translator.
				expect(entries[0].values[0].type).to.equal(TEST_CLIPBOARD_ENTRY_VALUE_TYPE);
				expect(entries[0].values[0].value).to.equal('hello clipboard value');
			});

			it('builds the entry name from workspace, property and item', async () => {
				providePropertyContext({ label: 'My Property' });
				provideDatasetContext('My Workspace');
				const context = await createContext();

				await context.write({ propertyValue: 'hello', itemName: 'My Item' });

				const entries = await readWrittenEntries();
				expect(entries[0].name).to.equal('My Workspace - My Property - My Item');
			});

			it('omits the item name when none is provided', async () => {
				providePropertyContext({ label: 'My Property' });
				provideDatasetContext('My Workspace');
				const context = await createContext();

				await context.write({ propertyValue: 'hello' });

				const entries = await readWrittenEntries();
				expect(entries[0].name).to.equal('My Workspace - My Property');
			});

			it('uses an explicit name instead of deriving one', async () => {
				providePropertyContext({ label: 'My Property' });
				provideDatasetContext('My Workspace');
				const context = await createContext();

				await context.write({ propertyValue: 'hello', name: 'A name of my own' });

				const entries = await readWrittenEntries();
				expect(entries[0].name).to.equal('A name of my own');
			});

			it('falls back to the icon of the property editor', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				await context.write({ propertyValue: 'hello' });

				const entries = await readWrittenEntries();
				expect(entries[0].icon).to.equal('icon-document');
			});

			it('waits for a property context that is provided after the call', async () => {
				provideDatasetContext('My Workspace');
				const context = await createContext();

				// This context is an extension behind a dynamic import, so the contexts it derives from can land in
				// either order — the write has to wait rather than fail.
				const writing = context.write({ propertyValue: 'hello', itemName: 'My Item' });
				providePropertyContext({ label: 'My Property' });
				await writing;

				const entries = await readWrittenEntries();
				expect(entries).to.have.lengthOf(1);
				expect(entries[0].name).to.equal('My Workspace - My Property - My Item');
			});

			it('prefers an explicitly passed alias over the derived one', async () => {
				providePropertyContext({ alias: TEST_PROPERTY_EDITOR_UI_ALIAS_WITHOUT_TRANSLATORS });
				provideDatasetContext();
				const context = await createContext();

				await context.write({
					propertyValue: 'hello',
					name: 'An entry written on behalf of another property editor',
					propertyEditorUiAlias: TEST_PROPERTY_EDITOR_UI_ALIAS,
				});

				const entries = await readWrittenEntries();
				expect(entries).to.have.lengthOf(1);
				expect(entries[0].values[0].type).to.equal(TEST_CLIPBOARD_ENTRY_VALUE_TYPE);
			});

			it('throws when the property editor UI alias cannot be resolved', async () => {
				providePropertyContext({ alias: undefined });
				provideDatasetContext();
				const context = await createContext();

				let error: unknown;
				try {
					await context.write({ propertyValue: 'hello' });
				} catch (e) {
					error = e;
				}
				expect(error).to.be.instanceOf(Error);
				expect(await readWrittenEntries()).to.have.lengthOf(0);
			});
		});

		describe('readMultiple', () => {
			it('translates entries back into values for the surrounding property editor', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				await context.write({ propertyValue: 'hello property value', itemName: 'My Item' });
				const [entry] = await readWrittenEntries();

				const propertyValues = await context.readMultiple<string>([entry.unique]);

				// Round-tripped through the copy translator on write and the paste translator on read.
				expect(propertyValues).to.deep.equal(['hello property value']);
			});
		});

		describe('getSupportedPasteEntryValueTypes', () => {
			it('returns the value types the property editor has a paste translator for', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				// Both registered paste translators target the test property editor; the unsupported type has none.
				const types = await context.getSupportedPasteEntryValueTypes();
				expect(types).to.have.members([TEST_CLIPBOARD_ENTRY_VALUE_TYPE, TEST_INCOMPATIBLE_CLIPBOARD_ENTRY_VALUE_TYPE]);
				expect(types).to.not.include(TEST_UNSUPPORTED_CLIPBOARD_ENTRY_VALUE_TYPE);
			});

			it('waits for a property context that is provided after the call', async () => {
				provideDatasetContext();
				const context = await createContext();

				// An empty result would be indistinguishable from "this editor cannot paste anything", so the call
				// has to wait for the context rather than answer early.
				const types = context.getSupportedPasteEntryValueTypes();
				providePropertyContext();

				expect(await types).to.have.members([
					TEST_CLIPBOARD_ENTRY_VALUE_TYPE,
					TEST_INCOMPATIBLE_CLIPBOARD_ENTRY_VALUE_TYPE,
				]);
			});

			it('throws when the property editor UI alias cannot be resolved', async () => {
				providePropertyContext({ alias: undefined });
				provideDatasetContext();
				const context = await createContext();

				let error: unknown;
				try {
					await context.getSupportedPasteEntryValueTypes();
				} catch (e) {
					error = e;
				}
				expect(error).to.be.instanceOf(Error);
			});
		});

		describe('isEntryPastable', () => {
			function entryWithValueType(type: string) {
				return { unique: 'entry-unique', values: [{ type, value: 'a clipboard value' }] } as any;
			}

			it('accepts an entry whose paste translator reports no compatibility constraint', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				expect(await context.isEntryPastable(entryWithValueType(TEST_CLIPBOARD_ENTRY_VALUE_TYPE))).to.be.true;
			});

			it('rejects an entry the paste translator reports as incompatible', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				expect(await context.isEntryPastable(entryWithValueType(TEST_INCOMPATIBLE_CLIPBOARD_ENTRY_VALUE_TYPE))).to.be
					.false;
			});

			it('rejects an entry of a type no paste translator targets, rather than throwing', async () => {
				providePropertyContext();
				provideDatasetContext();
				const context = await createContext();

				// Type filtering happens in the collection, so this is a defensive path — but it must not throw,
				// because it runs per entry while a list is being built.
				expect(await context.isEntryPastable(entryWithValueType(TEST_UNSUPPORTED_CLIPBOARD_ENTRY_VALUE_TYPE))).to.be
					.false;
			});
		});
	});
});
