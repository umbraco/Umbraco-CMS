import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityDataPickerInputContext } from './input-entity-data.context.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemDataResolver, UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbPickerDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

// A test entity model that has a label instead of name, simulating entities like Document
// where names are variant-dependent and not stored as a top-level property.
interface UmbTestItemModel extends UmbItemModel {
	label: string;
}

// A resolver that derives its display name from item.label, not from any constructor argument.
// This guarantees the resolver path is genuinely exercised — item.name is undefined for
// UmbTestItemModel, so any name returned must come through setData/label.
class UmbTestItemDataResolver extends UmbControllerBase implements UmbItemDataResolver<UmbTestItemModel> {
	#data = new UmbObjectState<UmbTestItemModel | undefined>(undefined);

	public readonly entityType = this.#data.asObservablePart((x) => x?.entityType);
	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.icon ?? undefined);

	#name = new UmbStringState(undefined);
	public readonly name: Observable<string | undefined> = this.#name.asObservable();

	setData(data: UmbTestItemModel | undefined) {
		this.#data.setValue(data);
		this.#name.setValue(data?.label);
	}

	getData(): UmbTestItemModel | undefined {
		return this.#data.getValue();
	}

	async getEntityType(): Promise<string | undefined> {
		return this.observe(this.entityType).asPromise();
	}

	async getUnique(): Promise<string | undefined> {
		return this.observe(this.unique).asPromise();
	}

	async getName(): Promise<string | undefined> {
		return this.#name.getValue();
	}

	async getIcon(): Promise<string | undefined> {
		return this.observe(this.icon).asPromise();
	}
}

class UmbTestPickerDataSource extends UmbControllerBase implements UmbPickerDataSource<UmbItemModel> {
	requestItems(): ReturnType<UmbPickerDataSource['requestItems']> {
		return Promise.resolve({ data: [] });
	}

	// requestCollection is required so isPickerCollectionDataSource returns true,
	// allowing #setModalToken to resolve without throwing.
	requestCollection() {
		return Promise.resolve({ data: { items: [], total: 0 } });
	}
}

class UmbTestPickerDataSourceWithResolver extends UmbTestPickerDataSource {
	createItemDataResolver(host: UmbControllerHost): UmbItemDataResolver {
		return new UmbTestItemDataResolver(host);
	}
}

@customElement('test-entity-data-picker-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Exposes the protected _requestItemName method and bypasses the item manager so
// tests do not need the repository extension to be registered.
class UmbTestEntityDataPickerInputContext extends UmbEntityDataPickerInputContext {
	#testItems = new Map<string, UmbItemModel>();

	setTestItem(item: UmbItemModel) {
		this.#testItems.set(item.unique, item);
	}

	override getSelectedItemByUnique(unique: string): UmbItemModel | undefined {
		return this.#testItems.get(unique);
	}

	async requestItemName(unique: string): Promise<string> {
		return this._requestItemName(unique);
	}
}

describe('UmbEntityDataPickerInputContext._requestItemName', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTestEntityDataPickerInputContext;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		context = new UmbTestEntityDataPickerInputContext(hostElement);
	});

	afterEach(() => {
		context.destroy();
		document.body.innerHTML = '';
	});

	it('should return the name resolved from item data by the data source resolver', async () => {
		const item: UmbTestItemModel = { unique: 'test-1', entityType: 'test', label: 'Resolved Label' };
		context.setTestItem(item);
		context.setDataSourceApi(new UmbTestPickerDataSourceWithResolver(hostElement));

		const name = await context.requestItemName('test-1');
		expect(name).to.equal('Resolved Label');
	});

	it('should fall back to item.name when the data source provides no resolver', async () => {
		const item: UmbItemModel = { unique: 'test-2', entityType: 'test', name: 'Item Name' };
		context.setTestItem(item);
		context.setDataSourceApi(new UmbTestPickerDataSource(hostElement));

		const name = await context.requestItemName('test-2');
		expect(name).to.equal('Item Name');
	});

	it('should fall back to item.name when the resolver returns undefined', async () => {
		// UmbItemModel has name but no label. UmbTestItemDataResolver reads .label, which is
		// undefined here, so getName() returns undefined — exercising the if(name) guard in
		// _requestItemName and ensuring it falls through to super._requestItemName.
		const item: UmbItemModel = { unique: 'test-3', entityType: 'test', name: 'Fallback Name' };
		context.setTestItem(item);
		context.setDataSourceApi(new UmbTestPickerDataSourceWithResolver(hostElement));

		const name = await context.requestItemName('test-3');
		expect(name).to.equal('Fallback Name');
	});

	it('should return #general_notFound when no item is found for the given unique', async () => {
		const name = await context.requestItemName('non-existent');
		expect(name).to.equal('#general_notFound');
	});
});
