import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityDataPickerDataSourceApiContext } from '../input/entity-data-picker-data-source.context.js';
import { UmbEntityDataPickerSupportsTextFilterCondition } from './entity-data-picker-supports-text-filter.condition.js';
import { UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS } from './constants.js';
import type { UmbPickerCollectionDataSourceTextFilterFeature } from '@umbraco-cms/backoffice/picker-data-source';

@customElement('test-controller-host-supports-text-filter')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-child-supports-text-filter')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

function createCollectionDataSourceMock(enabled: boolean) {
	const supportsTextFilter = new UmbObjectState<UmbPickerCollectionDataSourceTextFilterFeature>({ enabled });
	return {
		// requestCollection makes isPickerCollectionDataSource return true
		requestCollection: () => Promise.resolve({ data: { items: [], total: 0 } }),
		requestItems: () => Promise.resolve({ data: [] }),
		features: {
			supportsTextFilter: supportsTextFilter.asObservable(),
		},
		_supportsTextFilter: supportsTextFilter,
	};
}

function createNonCollectionDataSourceMock() {
	return {
		// No requestCollection — isPickerCollectionDataSource returns false
		requestItems: () => Promise.resolve({ data: [] }),
	};
}

describe('UmbEntityDataPickerSupportsTextFilterCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let condition: UmbEntityDataPickerSupportsTextFilterCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('should be permitted when data source supports text filter', (done) => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		const mock = createCollectionDataSourceMock(true);
		context.setDataSourceApi(mock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(condition.permitted).to.be.true;
					condition.hostDisconnected();
					done();
				}
			},
		});
	});

	it('should not be permitted when data source has text filter disabled', async () => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		const mock = createCollectionDataSourceMock(false);
		context.setDataSourceApi(mock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
			},
		});

		// permitted starts as false. Setting it to false does not trigger onChange.
		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
		expect(callbackCount).to.equal(0);
		condition.hostDisconnected();
	});

	it('should not be permitted when data source is not a collection data source', async () => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		const mock = createNonCollectionDataSourceMock();
		context.setDataSourceApi(mock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
			},
		});

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
		expect(callbackCount).to.equal(0);
		condition.hostDisconnected();
	});

	it('should not be permitted when collection data source has no features', async () => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		// Has requestCollection (passes isPickerCollectionDataSource) but no features property
		const mock = {
			requestCollection: () => Promise.resolve({ data: { items: [], total: 0 } }),
			requestItems: () => Promise.resolve({ data: [] }),
		};
		context.setDataSourceApi(mock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
			},
		});

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
		expect(callbackCount).to.equal(0);
		condition.hostDisconnected();
	});

	it('should not be permitted when no context is available', async () => {
		// No context provided on hostElement
		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
			},
		});

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
		expect(callbackCount).to.equal(0);
		condition.hostDisconnected();
	});

	it('should react when supportsTextFilter changes from enabled to disabled', (done) => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		const mock = createCollectionDataSourceMock(true);
		context.setDataSourceApi(mock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(condition.permitted).to.be.true;
					// Defer state change to avoid synchronous re-entry into onChange
					setTimeout(() => {
						mock._supportsTextFilter.setValue({ enabled: false });
					}, 0);
				}
				if (callbackCount === 2) {
					expect(condition.permitted).to.be.false;
					condition.hostDisconnected();
					done();
				}
			},
		});
	});

	it('should clean up observers of previous data source if switching to non-collection', (done) => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		const collectionMock = createCollectionDataSourceMock(true);
		context.setDataSourceApi(collectionMock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(condition.permitted).to.be.true;
					setTimeout(() => {
						context.setDataSourceApi(createNonCollectionDataSourceMock() as any);
					}, 0);
				}
				if (callbackCount === 2) {
					expect(condition.permitted).to.be.false;
					setTimeout(() => {
						// Emit on the OLD data source — must not affect permitted.
						// Two setValue calls needed because UmbDeepState deduplicates same-value emissions.
						collectionMock._supportsTextFilter.setValue({ enabled: false });
						collectionMock._supportsTextFilter.setValue({ enabled: true });
						setTimeout(() => {
							expect(condition.permitted).to.be.false;
							condition.hostDisconnected();
							done();
						}, 0);
					}, 0);
				}
			},
		});
	});

	it('should react when data source api changes', (done) => {
		const context = new UmbEntityDataPickerDataSourceApiContext(hostElement);
		const mock = createCollectionDataSourceMock(true);
		context.setDataSourceApi(mock as any);

		let callbackCount = 0;
		condition = new UmbEntityDataPickerSupportsTextFilterCondition(childElement, {
			host: childElement,
			config: { alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(condition.permitted).to.be.true;
					// Defer state change to avoid synchronous re-entry into onChange
					setTimeout(() => {
						const nonCollectionMock = createNonCollectionDataSourceMock();
						context.setDataSourceApi(nonCollectionMock as any);
					}, 0);
				}
				if (callbackCount === 2) {
					expect(condition.permitted).to.be.false;
					condition.hostDisconnected();
					done();
				}
			},
		});
	});
});
