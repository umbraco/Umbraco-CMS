import type {
	UmbClipboardCopyPropertyValueTranslator,
	UmbClipboardPastePropertyValueTranslator,
} from '../value-translator/types.js';
import { UmbClipboardPropertyContext } from '../context/clipboard.property-context.js';
import { UmbPropertyClipboardManager } from './property-clipboard.manager.js';
import { UmbClipboardCollectionRepository } from '../../collection/index.js';
import { UmbClipboardContext } from '../../context/clipboard.context.js';
import { UmbClipboardEntryDetailStore } from '../../clipboard-entry/index.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbPropertyContext, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

const TEST_PROPERTY_EDITOR_UI_ALIAS = 'Test.PropertyEditorUi';
const TEST_CLIPBOARD_ENTRY_VALUE_TYPE = 'testClipboardEntryValueType';

class UmbTestClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<string, string>
{
	async translate(propertyValue: string): Promise<string> {
		return propertyValue.replaceAll(' property value', '') + ' clipboard value';
	}
}

class UmbTestClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<string, string>
{
	async translate(clipboardEntryValue: string): Promise<string> {
		return clipboardEntryValue.replaceAll(' clipboard value', '') + ' property value';
	}
}

const copyTranslatorManifest = {
	type: 'clipboardCopyPropertyValueTranslator',
	alias: 'Test.ClipboardCopyPropertyValueTranslator',
	name: 'Test Clipboard Copy Property Value Translator',
	api: UmbTestClipboardCopyPropertyValueTranslator,
	fromPropertyEditorUi: TEST_PROPERTY_EDITOR_UI_ALIAS,
	toClipboardEntryValueType: TEST_CLIPBOARD_ENTRY_VALUE_TYPE,
};

const pasteTranslatorManifest = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Test.ClipboardPastePropertyValueTranslator',
	name: 'Test Clipboard Paste Property Value Translator',
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

@customElement('test-property-clipboard-manager-host')
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

function readCurrent<T>(observable: { subscribe: (cb: (value: T) => void) => { unsubscribe: () => void } }): T {
	let value!: T;
	const subscription = observable.subscribe((next) => (value = next));
	subscription.unsubscribe();
	return value;
}

describe('UmbPropertyClipboardManager', () => {
	let hostElement: UmbTestControllerHostElement;

	beforeEach(async () => {
		umbExtensionsRegistry.registerMany([copyTranslatorManifest, pasteTranslatorManifest, propertyEditorManifest]);
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		umbExtensionsRegistry.clear();
		localStorage.clear();
		document.body.innerHTML = '';
	});

	// Real contexts are used because the clipboard property context and the property context share the
	// 'UmbPropertyContext' base alias — only their real implementations register with the discriminator that
	// lets both resolve independently (as they do in production).
	function provideClipboardContext() {
		return new UmbClipboardPropertyContext(hostElement);
	}

	function providePropertyContext(options?: { alias?: string | undefined; label?: string }) {
		const propertyContext = new UmbPropertyContext(hostElement);
		// Intentionally no setAlias: the manager reads the editor UI alias (getEditorManifest), not the property
		// alias, and leaving it unset keeps UmbPropertyContext from observing the dataset stub's value methods.
		propertyContext.setLabel(options?.label ?? 'My Property');
		const alias = options && 'alias' in options ? options.alias : TEST_PROPERTY_EDITOR_UI_ALIAS;
		propertyContext.setEditorManifest(alias ? ({ alias } as any) : undefined);
		return propertyContext;
	}

	function provideDatasetContext(name = 'My Workspace') {
		const datasetContext = { getName: () => name, getHostElement: () => hostElement } as any;
		new UmbContextProvider(hostElement, UMB_PROPERTY_DATASET_CONTEXT, datasetContext).hostConnected();
	}

	async function createManager() {
		const manager = new UmbPropertyClipboardManager(hostElement);
		await aTimeout(0);
		return manager;
	}

	async function readWrittenEntries() {
		const { data } = await new UmbClipboardCollectionRepository(hostElement).requestCollection({
			types: [TEST_CLIPBOARD_ENTRY_VALUE_TYPE],
		});
		return data?.items ?? [];
	}

	describe('isAvailable', () => {
		it('is false when the clipboard property context is not provided', async () => {
			providePropertyContext();
			provideDatasetContext();
			const manager = await createManager();
			expect(readCurrent(manager.isAvailable)).to.be.false;
		});

		it('is true when the clipboard property context is provided', async () => {
			provideClipboardContext();
			providePropertyContext();
			provideDatasetContext();
			const manager = await createManager();
			expect(readCurrent(manager.isAvailable)).to.be.true;
		});
	});

	describe('write', () => {
		it('writes an entry using the alias derived from the property editor manifest', async () => {
			provideClipboardContext();
			providePropertyContext();
			provideDatasetContext();
			const manager = await createManager();

			await manager.write({ propertyValue: 'hello', itemName: 'My Item', icon: 'icon-picture' });

			const entries = await readWrittenEntries();
			expect(entries).to.have.lengthOf(1);
			expect(entries[0].icon).to.equal('icon-picture');
			// The value type is only produced when the alias resolves the copy translator.
			expect(entries[0].values[0].type).to.equal(TEST_CLIPBOARD_ENTRY_VALUE_TYPE);
			expect(entries[0].values[0].value).to.equal('hello clipboard value');
		});

		it('builds the entry name from workspace, property and item', async () => {
			provideClipboardContext();
			providePropertyContext({ label: 'My Property' });
			provideDatasetContext('My Workspace');
			const manager = await createManager();

			await manager.write({ propertyValue: 'hello', itemName: 'My Item' });

			const entries = await readWrittenEntries();
			expect(entries[0].name).to.equal('My Workspace - My Property - My Item');
		});

		it('omits the workspace name when no dataset context is present', async () => {
			provideClipboardContext();
			providePropertyContext({ label: 'My Property' });
			const manager = await createManager();

			await manager.write({ propertyValue: 'hello', itemName: 'My Item' });

			const entries = await readWrittenEntries();
			expect(entries[0].name).to.equal('My Property - My Item');
		});

		it('omits the item name when none is provided', async () => {
			provideClipboardContext();
			providePropertyContext({ label: 'My Property' });
			provideDatasetContext('My Workspace');
			const manager = await createManager();

			await manager.write({ propertyValue: 'hello' });

			const entries = await readWrittenEntries();
			expect(entries[0].name).to.equal('My Workspace - My Property');
		});

		it('throws when the clipboard context is not available', async () => {
			providePropertyContext();
			provideDatasetContext();
			const manager = await createManager();

			let error: unknown;
			try {
				await manager.write({ propertyValue: 'hello' });
			} catch (e) {
				error = e;
			}
			expect(error).to.be.instanceOf(Error);
		});

		it('throws when the property context is not available', async () => {
			provideClipboardContext();
			const manager = await createManager();

			let error: unknown;
			try {
				await manager.write({ propertyValue: 'hello' });
			} catch (e) {
				error = e;
			}
			expect(error).to.be.instanceOf(Error);
		});

		it('throws when the property editor UI alias cannot be resolved', async () => {
			provideClipboardContext();
			providePropertyContext({ alias: undefined });
			provideDatasetContext();
			const manager = await createManager();

			let error: unknown;
			try {
				await manager.write({ propertyValue: 'hello' });
			} catch (e) {
				error = e;
			}
			expect(error).to.be.instanceOf(Error);
			expect(await readWrittenEntries()).to.have.lengthOf(0);
		});
	});
});
