import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardContext } from './clipboard.context.js';
import { UmbClipboardEntryDetailStore, type UmbClipboardEntryDetailModel } from '../clipboard-entry/index.js';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

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
		hostElement = new UmbTestControllerHostElement();
		clipboardContext = new UmbClipboardContext(hostElement);
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		localStorage.clear();
		document.body.innerHTML = '';
	});

	describe('write', () => {
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
});
