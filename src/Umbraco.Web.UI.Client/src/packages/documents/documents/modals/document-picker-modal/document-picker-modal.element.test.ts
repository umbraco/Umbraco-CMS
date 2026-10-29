import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/constants.js';
import { UmbDocumentPickerModalElement } from './document-picker-modal.element.js';
import type { UmbDocumentPickerModalData } from './types.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeItemPickerContext } from '@umbraco-cms/backoffice/tree';

const TREE_REPOSITORY_ALIAS = 'Umb.Test.DocumentPickerModal.TreeRepository';

class UmbTestTreeRepository {
	async requestTreeRoot() {
		return { data: { unique: null, entityType: 'test-root', name: 'Root' } };
	}

	async requestTreeItemAncestors() {
		return { data: [] };
	}

	destroy() {}
}

describe('UmbDocumentPickerModalElement', () => {
	let element: UmbDocumentPickerModalElement;
	let restoreErrorHandler: () => void;

	const pickableFilter = () => true;

	beforeEach(async () => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();

		umbExtensionsRegistry.registerMany([
			{ type: 'repository', alias: TREE_REPOSITORY_ALIAS, name: 'Test Tree Repository', api: UmbTestTreeRepository },
			{
				type: 'tree',
				alias: UMB_DOCUMENT_TREE_ALIAS,
				name: 'Test Document Tree',
				meta: { repositoryAlias: TREE_REPOSITORY_ALIAS },
			},
		] as Array<never>);

		const host = await fixture<HTMLElement>(html`<div></div>`);
		element = new UmbDocumentPickerModalElement();
		element.data = { multiple: true, pickableFilter } as UmbDocumentPickerModalData;
		host.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(UMB_DOCUMENT_TREE_ALIAS);
		umbExtensionsRegistry.unregister(TREE_REPOSITORY_ALIAS);
		restoreErrorHandler();
	});

	const getPicker = () => element.shadowRoot!.querySelector('umb-document-picker-browse')!;
	const pickerContext = () => (element as unknown as { _pickerContext: UmbTreeItemPickerContext })._pickerContext;

	it('browses with umb-document-picker-browse', () => {
		expect(getPicker()).to.exist;
	});

	it('hands the pickable filter to umb-document-picker-browse', () => {
		expect(getPicker().pickableFilter).to.equal(pickableFilter);
	});

	it('browses the document tree without being given a tree alias', async () => {
		await waitUntil(() => pickerContext().location.getCurrentLocation() !== undefined, 'no location was established');

		expect(pickerContext().location.getCurrentLocation()?.entityType).to.equal('test-root');
	});

	it('configures the picker context for multiple selection from the modal data', () => {
		expect(pickerContext().selection.getMultiple()).to.be.true;
	});

	// The modal's `value` is written through the modal context, which a standalone element has none of, so the
	// selection is read from the picker context the modal owns.
	it('takes a selection made in umb-document-picker-browse', async () => {
		await waitUntil(() => !!getPicker().shadowRoot?.querySelector('umb-tree'), 'the tree was never rendered');

		getPicker().shadowRoot!.querySelector('umb-tree')!.dispatchEvent(new UmbSelectedEvent('picked'));
		await element.updateComplete;

		expect(pickerContext().selection.getSelection()).to.include('picked');
	});

	describe('options the document picker does not apply', () => {
		let warnings: Array<string>;
		const originalWarn = console.warn;

		beforeEach(() => {
			warnings = [];
			console.warn = (...args: unknown[]) => {
				warnings.push(String(args[0]));
			};
		});

		afterEach(() => {
			console.warn = originalWarn;
		});

		const warningsFor = (option: string) =>
			warnings.filter((warning) => warning.includes(`\`${option}\` option of UmbDocumentPickerModalData`));

		it('warns when foldersOnly is passed', async () => {
			element.data = { ...element.data, foldersOnly: true } as UmbDocumentPickerModalData;
			await element.updateComplete;

			expect(warningsFor('foldersOnly')).to.have.lengthOf(1);
		});

		it('warns when createAction is passed', async () => {
			element.data = {
				...element.data,
				createAction: { label: 'Create' },
			} as unknown as UmbDocumentPickerModalData;
			await element.updateComplete;

			expect(warningsFor('createAction')).to.have.lengthOf(1);
		});

		it('does not warn for options it applies', async () => {
			element.data = { ...element.data, multiple: false } as UmbDocumentPickerModalData;
			await element.updateComplete;

			expect(warningsFor('foldersOnly')).to.have.lengthOf(0);
			expect(warningsFor('createAction')).to.have.lengthOf(0);
		});
	});
});
