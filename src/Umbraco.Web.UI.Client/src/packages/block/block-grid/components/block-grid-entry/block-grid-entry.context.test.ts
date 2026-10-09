import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../../types.js';
import { UmbBlockGridEntryContext } from './block-grid-entry.context.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { of } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, UmbNumberState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-block-grid-entry-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBlockGridEntryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * Drives the entry context's layout/block type state directly, standing in for the manager and entries
 * contexts, so the order in which the layout and the block type arrive can be controlled.
 */
class UmbTestBlockGridEntryContext extends UmbBlockGridEntryContext {
	#layoutColumns = new UmbNumberState<number | undefined>(undefined);

	connect() {
		const entries = {
			layoutColumns: this.#layoutColumns.asObservable(),
			getLayoutColumns: () => this.#layoutColumns.getValue(),
			isBlockTypeAllowed: () => new UmbBooleanState(true).asObservable(),
		};
		const manager = {
			isSortMode: new UmbBooleanState(false).asObservable(),
			contentOf: () => of(undefined),
			isExternalContentOf: () => of(false),
			externalContentStateOf: () => of(undefined),
		};
		this._entries = entries as never;
		this._manager = manager as never;
		this._gotEntries();
	}

	setLayoutColumns(layoutColumns: number) {
		this.#layoutColumns.setValue(layoutColumns);
	}

	setLayout(layout: UmbBlockGridLayoutModel) {
		this._layout.setValue(layout);
	}

	setBlockType(blockType: Partial<UmbBlockGridTypeModel>) {
		this._blockType.setValue(blockType as UmbBlockGridTypeModel);
	}
}

const layoutWithColumnSpan = (columnSpan: number): UmbBlockGridLayoutModel => ({
	key: 'layout-key',
	contentKey: 'content-key',
	columnSpan,
	rowSpan: 1,
});

describe('UmbBlockGridEntryContext', () => {
	describe('column span correction', () => {
		let host: UmbControllerHostElement;
		let context: UmbTestBlockGridEntryContext;

		beforeEach(async () => {
			host = await fixture(html`<umb-test-block-grid-entry-host></umb-test-block-grid-entry-host>`);
			context = new UmbTestBlockGridEntryContext(host);
			context.connect();
			context.setLayoutColumns(12);
		});

		afterEach(() => {
			context.destroy();
		});

		it('keeps the column span while the block type has not loaded yet', () => {
			context.setLayout(layoutWithColumnSpan(4));

			expect(context.getColumnSpan()).to.equal(4);
		});

		it('keeps a column span that matches an option once the block type has loaded', () => {
			context.setLayout(layoutWithColumnSpan(4));

			context.setBlockType({ columnSpanOptions: [{ columnSpan: 4 }, { columnSpan: 12 }] });

			expect(context.getColumnSpan()).to.equal(4);
		});

		it('falls back to the layout columns when the loaded block type has no column span options', () => {
			context.setLayout(layoutWithColumnSpan(4));

			context.setBlockType({ columnSpanOptions: [] });

			expect(context.getColumnSpan()).to.equal(12);
		});
	});
});
