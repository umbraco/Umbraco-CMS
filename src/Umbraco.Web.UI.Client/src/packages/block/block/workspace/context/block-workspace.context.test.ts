import { UmbBlockManagerContext } from '../../context/block-manager.context.js';
import { UMB_BLOCK_ENTRIES_CONTEXT } from '../../context/block-entries.context-token.js';
import type { UmbBlockDataModel, UmbBlockLayoutBaseModel } from '../../types.js';
import { UmbElementDetailStore } from '../../../../elements/repository/detail/element-detail.store.js';
import { UmbBlockWorkspaceContext } from './block-workspace.context.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';

const CONTENT_TYPE_KEY = 'b818bb55-31e1-4537-9c42-17471a176089';
const SETTINGS_TYPE_KEY = '5b3e9d7a-2c14-4a68-8f0b-6d1e4a9c7b23';
const SHARED_CONTENT_KEY = 'shared-content';

@customElement('umb-test-block-workspace-host')
class UmbTestBlockWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbActionEventContext(this);
		new UmbElementDetailStore(this);
	}
}

class UmbTestBlockManagerContext extends UmbBlockManagerContext {
	createWithPresets(): Promise<undefined> {
		throw new Error('Not used in this test.');
	}
	insert(): boolean {
		throw new Error('Not used in this test.');
	}
}

/**
 * The slice of the entries context that the block workspace relies on: the layout entries and lookups by layout key.
 */
class UmbTestBlockEntriesContext extends UmbContextBase {
	readonly #layoutEntries = new UmbArrayState<UmbBlockLayoutBaseModel>([], (x) => x.key);
	readonly layoutEntries = this.#layoutEntries.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_ENTRIES_CONTEXT.toString());
	}

	setLayouts(layouts: Array<UmbBlockLayoutBaseModel>) {
		this.#layoutEntries.setValue(layouts);
	}

	getLayouts() {
		return this.#layoutEntries.getValue();
	}

	layoutOf(contentKey: string) {
		return this.#layoutEntries.asObservablePart((source) => source.find((x) => x.contentKey === contentKey));
	}

	byKey(key: string) {
		return this.#layoutEntries.asObservablePart((source) => source.find((x) => x.key === key));
	}
}

const manifest = { alias: 'Umb.Workspace.Test.Block', meta: { entityType: 'block' } } as unknown as ManifestWorkspace;

const settingsData = (key: string, textSize: string): UmbBlockDataModel => ({
	key,
	contentTypeKey: SETTINGS_TYPE_KEY,
	values: [{ editorAlias: 'Umbraco.TextBox', culture: null, segment: null, alias: 'textSize', value: textSize }],
});

describe('UmbBlockWorkspaceContext', () => {
	describe('placements sharing a contentKey', () => {
		let host: UmbTestBlockWorkspaceHostElement;
		let manager: UmbTestBlockManagerContext;
		let entries: UmbTestBlockEntriesContext;
		let workspace: UmbBlockWorkspaceContext;

		before(async () => {
			await useMockSet('blocksReusableContent');
		});

		beforeEach(() => {
			host = new UmbTestBlockWorkspaceHostElement();
			document.body.appendChild(host);

			manager = new UmbTestBlockManagerContext(host);
			entries = new UmbTestBlockEntriesContext(host);

			const layouts: Array<UmbBlockLayoutBaseModel> = [
				{ key: 'layout-a', contentKey: SHARED_CONTENT_KEY, settingsKey: 'settings-a' },
				{ key: 'layout-b', contentKey: SHARED_CONTENT_KEY, settingsKey: 'settings-b' },
			];
			manager.setLayouts(layouts);
			manager.setContents([{ key: SHARED_CONTENT_KEY, contentTypeKey: CONTENT_TYPE_KEY, values: [] }]);
			manager.setSettings([settingsData('settings-a', 'Small'), settingsData('settings-b', 'Large')]);
			entries.setLayouts(layouts);

			workspace = new UmbBlockWorkspaceContext(host, { manifest });
		});

		afterEach(() => {
			workspace.destroy();
			entries.destroy();
			manager.destroy();
			document.body.innerHTML = '';
		});

		it('resolves each placement by its layout key', async () => {
			await workspace.load('layout-b');
			await waitUntil(() => workspace.settings.getData()?.key === 'settings-b', 'Settings of layout-b were not loaded');

			expect(workspace.getUnique()).to.equal('layout-b');
			expect(workspace.getData()?.settingsKey).to.equal('settings-b');
			expect(workspace.settings.getData()?.values[0].value).to.equal('Large');

			await workspace.load('layout-a');
			await waitUntil(() => workspace.settings.getData()?.key === 'settings-a', 'Settings of layout-a were not loaded');

			expect(workspace.getUnique()).to.equal('layout-a');
			expect(workspace.getData()?.settingsKey).to.equal('settings-a');
			expect(workspace.settings.getData()?.values[0].value).to.equal('Small');
		});
	});
});
