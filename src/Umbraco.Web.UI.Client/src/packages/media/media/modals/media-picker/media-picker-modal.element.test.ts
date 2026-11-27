import { UmbMediaPickerModalElement } from './media-picker-modal.element.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from './media-picker-modal.token.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/** kitchen-sink root items: a folder and an image. */
const FOLDER_ID = '5deac19f-5ca8-4b8c-a784-26593cec8d51';
const FOLDER_CHILD_ID = '95a8b7fc-cfec-4fb4-bce2-2dd1bc4818b4';
const IMAGE_ID = '76c02ec8-6a82-4c47-95da-56f6628b58fb';

type UmbTestMediaPickerModalElement = UmbMediaPickerModalElement & {
	_currentView: string;
	_currentChildren: Array<{ unique: string }>;
	_isSelectionMode: boolean;
};

/** Stands in for the modal context, which holds the modal's value. */
function createModalContext(value: UmbMediaPickerModalValue) {
	const state = new UmbObjectState(value);
	return {
		value: state.asObservable(),
		setValue: (newValue: UmbMediaPickerModalValue) => state.setValue(newValue),
		updateValue: (partialValue: Partial<UmbMediaPickerModalValue>) => state.update(partialValue),
		submit: () => {},
		reject: () => {},
	};
}

async function createModal(value: UmbMediaPickerModalValue) {
	const element = await fixture<UmbTestMediaPickerModalElement>(
		html`<umb-media-picker-modal
			.data=${{ multiple: true } as UmbMediaPickerModalData}
			.modalContext=${createModalContext(value) as never}></umb-media-picker-modal>`,
	);
	await element.updateComplete;
	await waitUntil(() => element._currentChildren.length > 0, 'the root items were never loaded');
	return element;
}

async function openTable(element: UmbTestMediaPickerModalElement) {
	element._currentView = 'table';
	await element.updateComplete;
	await waitUntil(() => element.shadowRoot!.querySelector('umb-table'), 'the table was never rendered');
	return element.shadowRoot!.querySelector('umb-table')!;
}

function getRow(table: Element, id: string): HTMLElement {
	return table.shadowRoot!.querySelector(`uui-table-row[data-sortable-id="${id}"]`) as HTMLElement;
}

function getNavigateButton(row: HTMLElement): HTMLElement | null {
	const nameCell = row.querySelector('umb-media-picker-table-column-name');
	return nameCell?.shadowRoot?.querySelector('uui-button') ?? null;
}

describe('UmbMediaPickerModalElement', () => {
	let element: UmbTestMediaPickerModalElement;
	let restoreErrorHandler: () => void;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(async () => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();
	});

	afterEach(() => {
		element?.remove();
		restoreErrorHandler();
	});

	it('opens folders from the table view while a preselection exists', async () => {
		element = await createModal({ selection: [IMAGE_ID] });
		const table = await openTable(element);

		const folderRow = getRow(table, FOLDER_ID);
		expect(folderRow.hasAttribute('select-only'), 'the folder row must stay interactive').to.be.false;
		expect(folderRow.hasAttribute('data-selection-mode'), 'the folder row must keep its icon').to.be.false;
		expect(getNavigateButton(folderRow), 'the folder name must render as an open button').to.not.be.null;

		getNavigateButton(folderRow)!.click();
		await waitUntil(() => getRow(table, FOLDER_CHILD_ID), 'the folder never opened');
	});

	it('keeps preselected rows selectable while folders are navigable', async () => {
		element = await createModal({ selection: [IMAGE_ID] });
		const table = await openTable(element);

		const imageRow = getRow(table, IMAGE_ID);
		expect(imageRow.hasAttribute('select-only')).to.be.true;
		expect(imageRow.querySelector('uui-checkbox')).to.exist;
	});

	it('returns folders to selection mode once the user selects within the dialog', async () => {
		element = await createModal({ selection: [] });
		const table = await openTable(element);

		const imageCheckbox = getRow(table, IMAGE_ID).querySelector('uui-checkbox') as HTMLInputElement;
		imageCheckbox.click();
		await element.updateComplete;
		await waitUntil(() => element._isSelectionMode === true);

		const folderRow = getRow(table, FOLDER_ID);
		expect(folderRow.hasAttribute('select-only')).to.be.true;
	});
});
