import { expect, oneEvent } from '@open-wc/testing';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { customElement } from 'lit/decorators';
import { UmbSorterConfig, UmbSorterController } from './sorter.controller';

type SortEntryType = {
	id: string;
	value: string;
};

const sorterConfig: UmbSorterConfig<SortEntryType> = {
	compareElementToModel: (element: HTMLElement, model: SortEntryType) => {
		return element.getAttribute('id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: SortEntryType) => {
		return container.querySelector('data-sort-entry-id[' + modelEntry.id + ']');
	},
	identifier: 'test-sorter',
	itemSelector: 'li',
};

const model: Array<SortEntryType> = [
	{
		id: '0',
		value: 'Entry 0',
	},
	{
		id: '1',
		value: 'Entry 1',
	},
	{
		id: '2',
		value: 'Entry 2',
	},
];

@customElement('test-my-sorter-controller')
class MyTestSorterControllerElement extends UmbLitElement {
	public sorter;

	constructor() {
		super();

		this.sorter = new UmbSorterController(this, sorterConfig);
		this.sorter.setModel(model);
	}
}

describe('UmbContextConsumer', () => {
	let hostElement: MyTestSorterControllerElement;
	let controller: UmbSorterController<SortEntryType>;

	beforeEach(() => {
		hostElement = document.createElement('test-my-sorter-controller') as MyTestSorterControllerElement;
		controller = (hostElement as any).__umbBlockGridSorterController();
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('element gets a internal public method on to retrieve controller', () => {
				//expect(hostElement).to.have.property('__umbBlockGridSorterController').that.is.a('function');
				expect(controller).to.not.eq(undefined);
			});
		});
	});

	// TODO: Testing ideas:
	// - Test that the model is updated correctly?
	// - Test that the DOM is updated correctly?
	// - Use the controller to sort the DOM and test that the model is updated correctly...
});
